using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SimpleBitware.AspectNet.Generator;

[Generator]
public sealed class AspectGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new AspectSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        Debugger.Launch();

        if (context.SyntaxReceiver is not AspectSyntaxReceiver receiver)
            return;



        var compilation = context.Compilation;

        //
        // Collect methods grouped by type
        //
        var methodsByType = new Dictionary<INamedTypeSymbol, List<IMethodSymbol>>(SymbolEqualityComparer.Default);

        //
        // Collect aspect descriptor expressions per method
        //
        var aspectMap = new Dictionary<IMethodSymbol, List<string>>(SymbolEqualityComparer.Default);

        foreach (var methodSyntax in receiver.Candidates)
        {
            var model = compilation.GetSemanticModel(methodSyntax.SyntaxTree);
            if (model.GetDeclaredSymbol(methodSyntax) is not IMethodSymbol methodSymbol)
                continue;

            // Extract attributes
            var attributes = methodSymbol.GetAttributes();

            // Convert attributes → descriptor expressions
            var descriptorExpressions = attributes
                .Where(a => IsLogAttribute(a.AttributeClass))
                .Select(CreateAspectDescriptorFromAttribute) // <-- NEW
                .ToList();

            if (descriptorExpressions.Count == 0)
                continue;

            // Add to aspect map
            aspectMap[methodSymbol] = descriptorExpressions;

            // Group by type
            var type = methodSymbol.ContainingType;
            if (!methodsByType.TryGetValue(type, out var list))
            {
                list = new List<IMethodSymbol>();
                methodsByType[type] = list;
            }

            list.Add(methodSymbol);
        }

        if (methodsByType.Count == 0)
            return;

        // ---------------------------------------------------------
        // DI REGISTRATION BUILDER
        // ---------------------------------------------------------
        var diBuilder = new StringBuilder();
        diBuilder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        diBuilder.AppendLine("using SimpleBitware.AspectNet.Runtime;");
        diBuilder.AppendLine("using SimpleBitware.AspectNet.Runtime.Aspects;");
        diBuilder.AppendLine("using SimpleBitware.AspectNet.Attributes;");
        diBuilder.AppendLine("using SimpleBitware.AspectNet.Runtime.Configuration;");
        diBuilder.AppendLine();
        diBuilder.AppendLine("namespace SimpleBitware.AspectNet.Runtime.Generated;");
        diBuilder.AppendLine();
        diBuilder.AppendLine("public static partial class AopGeneratedRegistrations");
        diBuilder.AppendLine("{");
        diBuilder.AppendLine("    public static IServiceCollection AddAop(this IServiceCollection services)");
        diBuilder.AppendLine("    {");
        diBuilder.AppendLine("        services.AddAopCore();");
        diBuilder.AppendLine("        services.AddSingleton<IAspectRegistry>(provider =>");
        diBuilder.AppendLine("        {");
        diBuilder.AppendLine("              var registry = new AspectRegistry();");
        diBuilder.AppendLine("              AopGeneratedPipeline.RegisterGeneratedAspects(registry);");
        diBuilder.AppendLine("              return registry;");
        diBuilder.AppendLine("        });");

        // ---------------------------------------------------------
        // PIPELINE BUILDER
        // ---------------------------------------------------------
        var pipelineBuilder = new StringBuilder();
        pipelineBuilder.AppendLine("using SimpleBitware.AspectNet.Runtime;");
        pipelineBuilder.AppendLine("using SimpleBitware.AspectNet.Runtime.Aspects;");
        pipelineBuilder.AppendLine("using SimpleBitware.AspectNet.Attributes;");
        pipelineBuilder.AppendLine("using System;");
        pipelineBuilder.AppendLine();
        pipelineBuilder.AppendLine("namespace SimpleBitware.AspectNet.Runtime.Generated;");
        pipelineBuilder.AppendLine();
        pipelineBuilder.AppendLine("public static partial class AopGeneratedPipeline");
        pipelineBuilder.AppendLine("{");
        pipelineBuilder.AppendLine("    public static void RegisterGeneratedAspects(IAspectRegistry registry)");
        pipelineBuilder.AppendLine("    {");

        // ---------------------------------------------------------
        // GENERATE PROXIES PER TYPE
        // ---------------------------------------------------------
        foreach (var kvp in methodsByType)
        {
            var typeSymbol = kvp.Key;
            var methods = kvp.Value;

            var proxySource = GenerateProxyForType(
                typeSymbol,
                methods,
                aspectMap, // <-- NEW
                diBuilder,
                pipelineBuilder
            );

            context.AddSource(
                $"{SanitizeFileName(typeSymbol.ToDisplayString())}.AopProxy.g.cs",
                proxySource
            );
        }

        // ---------------------------------------------------------
        // CLOSE DI + PIPELINE
        // ---------------------------------------------------------
        diBuilder.AppendLine("        return services;");
        diBuilder.AppendLine("    }");
        diBuilder.AppendLine("}");
        context.AddSource("AopGeneratedRegistrations.g.cs", diBuilder.ToString());

        pipelineBuilder.AppendLine("    }");
        pipelineBuilder.AppendLine("}");
        context.AddSource("AopGeneratedPipeline.g.cs", pipelineBuilder.ToString());
    }

    // ---------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------

    private static bool IsLogAttribute(INamedTypeSymbol? attr)
    {
        if (attr is null)
            return false;

        while (attr != null)
        {
            if (attr.Name == "LogAttribute")
                return true;

            attr = attr.BaseType;
        }

        return false;
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = System.IO.Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(name.Length);

        foreach (var ch in name)
            sb.Append(invalid.Contains(ch) ? '_' : ch);

        return sb.ToString();
    }

    // ---------------------------------------------------------
    // PROXY GENERATION
    // ---------------------------------------------------------

    private static string GenerateProxyForType(
        INamedTypeSymbol typeSymbol,
        List<IMethodSymbol> methods,
        Dictionary<IMethodSymbol, List<string>> aspectMap,
        StringBuilder diBuilder,
        StringBuilder pipelineBuilder)
    {
        var ns = typeSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : typeSymbol.ContainingNamespace.ToDisplayString();

        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using SimpleBitware.AspectNet.Runtime.Aspects;");

        if (ns is not null)
        {
            sb.AppendLine();
            sb.AppendLine($"namespace {ns};");
        }

        var typeName = typeSymbol.Name;
        var proxyName = typeName + "_AopProxy";

        var interfaces = typeSymbol.AllInterfaces;
        var mainInterface = interfaces.FirstOrDefault();
        var interfaceTypeName = mainInterface?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        sb.AppendLine();
        sb.Append("public sealed class ").Append(proxyName);
        if (interfaceTypeName is not null)
            sb.Append(" : ").Append(interfaceTypeName);
        sb.AppendLine();
        sb.AppendLine("{");

        sb.AppendLine($"    private readonly {typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} _inner;");
        sb.AppendLine("    private readonly IAspectPipeline _pipeline;");

        sb.AppendLine();
        sb.AppendLine($"    public {proxyName}(" +
                      $"{typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} inner, " +
                      "IAspectPipeline pipeline)");
        sb.AppendLine("    {");
        sb.AppendLine("        _inner = inner;");
        sb.AppendLine("        _pipeline = pipeline;");
        sb.AppendLine("    }");

        //
        // Generate proxy methods with descriptor expressions
        //
        foreach (var method in methods)
        {
            var descriptors = aspectMap.TryGetValue(method, out var list)
                ? list
                : new List<string>();

            GenerateProxyMethod(method, sb, pipelineBuilder, typeSymbol, descriptors); // <-- UPDATED
        }

        sb.AppendLine("}");

        //
        // DI registration
        //
        if (interfaceTypeName is not null)
        {
            diBuilder.AppendLine(
                $"        services.AddTransient<{typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>();");

            diBuilder.AppendLine(
                $"        services.AddTransient<{interfaceTypeName}, {typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}_AopProxy>();");
        }

        return sb.ToString();
    }

    private static void GenerateProxyMethod(
        IMethodSymbol method,
        StringBuilder sb,
        StringBuilder pipelineBuilder,
        INamedTypeSymbol typeSymbol,
        IEnumerable<string> aspectDescriptors // <-- now C# expressions
    )
    {
        var returnType = method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var methodName = method.Name;

        var parameters = string.Join(", ",
            method.Parameters.Select(p =>
                $"{p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {p.Name}"));

        var args = string.Join(", ", method.Parameters.Select(p => p.Name));

        // Build method ID
        var methodId =
            $"{typeSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)}.{methodName}";

        //
        // Emit pipeline registration
        //
        var descriptorsJoined = string.Join(", ", aspectDescriptors);

        pipelineBuilder.AppendLine(
            $"        registry.Register(\"{methodId}\", {descriptorsJoined});");

        //
        // Emit proxy method
        //
        sb.AppendLine();
        sb.AppendLine($"    public {returnType} {methodName}({parameters})");
        sb.AppendLine("    {");

        // Build args array
        sb.AppendLine("        var __args = new object[] { " +
                      string.Join(", ", method.Parameters.Select(p => p.Name)) +
                      " };");

        // Async detection
        bool isTask = returnType == "global::System.Threading.Tasks.Task";
        bool isTaskT = returnType.StartsWith("global::System.Threading.Tasks.Task<");

        if (isTask)
        {
            sb.AppendLine(
                $"        return _pipeline.InvokeAsync(\"{methodId}\", _inner, __args, async () => await _inner.{methodName}({args}));");
        }
        else if (isTaskT)
        {
            sb.AppendLine(
                $"        return ({returnType})_pipeline.InvokeAsync(\"{methodId}\", _inner, __args, () => _inner.{methodName}({args}));");
        }
        else if (returnType == "void")
        {
            sb.AppendLine(
                $"        _pipeline.Invoke(\"{methodId}\", _inner, __args, () => _inner.{methodName}({args}));");
        }
        else
        {
            sb.AppendLine(
                $"        return ({returnType})_pipeline.Invoke(\"{methodId}\", _inner, __args, () => _inner.{methodName}({args}));");
        }

        sb.AppendLine("    }");
    }

    private static string CreateAspectDescriptorFromAttribute(AttributeData attribute)
    {
        var attributeName = attribute.AttributeClass!
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        // Map attribute → aspect
        string aspectType =
            attributeName.EndsWith(".LogAttribute")
                ? "global::SimpleBitware.AspectNet.Attributes.LogMethodAspect"
                : throw new InvalidOperationException($"Unknown aspect attribute: {attributeName}");

        // Extract constructor args (if any)
        var args = attribute.ConstructorArguments
            .Select(ConvertTypedConstant)
            .ToList();

        // If no constructor args, fall back to property
        if (args.Count == 0)
        {
            var categoryProp = attribute.NamedArguments
                .FirstOrDefault(kvp => kvp.Key == "Category")
                .Value;

            if (categoryProp.Value is string s)
                args.Add($"\"{s}\"");
            else
                args.Add("null");
        }

        var argsJoined = string.Join(", ", args);

        return
            $"new global::SimpleBitware.AspectNet.Runtime.Aspects.AspectDescriptor(" +
            $"typeof({aspectType}), {argsJoined})";
    }

    private static string ConvertTypedConstant(TypedConstant constant)
    {
        if (constant.IsNull)
            return "null";

        switch (constant.Kind)
        {
            case TypedConstantKind.Primitive:
                return constant.Value switch
                {
                    string s => $"\"{s}\"",
                    char c => $"'{c}'",
                    bool b => b ? "true" : "false",
                    _ => constant.Value!.ToString()!
                };

            case TypedConstantKind.Enum:
                return $"{constant.Type!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{constant.Value}";

            case TypedConstantKind.Type:
                var ts = (ITypeSymbol)constant.Value!;
                return $"typeof({ts.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})";

            case TypedConstantKind.Array:
                var items = constant.Values.Select(ConvertTypedConstant);
                return $"new [] {{ {string.Join(", ", items)} }}";

            default:
                throw new NotSupportedException(
                    $"Unsupported attribute argument kind: {constant.Kind}");
        }
    }
}