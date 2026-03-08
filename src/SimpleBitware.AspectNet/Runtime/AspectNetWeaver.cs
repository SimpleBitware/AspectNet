using Microsoft.Build.Utilities;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using SimpleBitware.AspectNet.Abstractions;
using SimpleBitware.AspectNet.Build;
using SimpleBitware.AspectNet.Extensions;
using SimpleBitware.AspectNet.Extensions.Cecil;

namespace SimpleBitware.AspectNet.Runtime;

public static class AspectNetWeaver
{
    public static string[] Run(string assemblyPath)
    {
        var pdbFilePath = GetPdbFilePath(assemblyPath);
        var targetAssemblyDirectory = GetTargetAssemblyDirectory(assemblyPath)!;
        var readerParams = GetReaderParameters(targetAssemblyDirectory, pdbFilePath);
        
        return ProcessModule(assemblyPath, pdbFilePath, readerParams);
    }

    private static string? GetPdbFilePath(string assemblyPath) => Path.ChangeExtension(assemblyPath, "pdb");

    private static string? GetTargetAssemblyDirectory(string assemblyPath) => Path.GetDirectoryName(assemblyPath);

    private static ReaderParameters GetReaderParameters(string targetAssemblyPath, string? pdbFilePath)
    {
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(targetAssemblyPath);

        return new ReaderParameters
        {
            ReadSymbols = File.Exists(pdbFilePath),
            ReadWrite = false,
            AssemblyResolver = resolver
        };
    }

    private static string[] ProcessModule(string assemblyPath, string? pdbFilePath, ReaderParameters readerParams)
    {
        using var assemblyStream = new MemoryStream();
        using var pdbStream = new MemoryStream();
        using (var module = ModuleDefinition.ReadModule(assemblyPath, readerParams))
        {
            var baseAspectAttribute = module
                .ImportReference(typeof(AbstractAspectNetAttribute))
                .Resolve();

            foreach (var type in module.Types)
            {
                ProcessType(module, type, baseAspectAttribute);
            }

            var writerParams = new WriterParameters
            {
                WriteSymbols = readerParams.ReadSymbols,
                SymbolStream = pdbStream,
                SymbolWriterProvider = new PortablePdbWriterProvider()
            };

            module.Write(assemblyStream, writerParams);
        }

        return new[]
            {
                assemblyStream.SaveToFile(assemblyPath),
                pdbStream.SaveToFile(pdbFilePath)
            }
            .Where(x => x != null).ToArray()!;
    }

    private static void ProcessType(ModuleDefinition module, TypeDefinition type, TypeDefinition baseAspectAttribute)
    {
        // 1. Process methods and constructors
        foreach (var method in type.Methods.Where(m => m.HasBody))
        {
            var aspectAttrs = method.CustomAttributes
                .Where(a => a.AttributeType.Resolve().InheritsFrom(baseAspectAttribute))
                .ToList();

            if (aspectAttrs.Count > 0)
                WeaveMethod(module, method, aspectAttrs);
        }

        // 2. Process Properties
        foreach (var property in type.Properties)
        {
            // Find attributes on the property itself
            var aspectAttrs = property.CustomAttributes
                .Where(a => a.AttributeType.Resolve()?.InheritsFrom(baseAspectAttribute) ?? false)
                .ToList();

            if (aspectAttrs.Count == 0) continue;
            // Apply to the Getter if it exists
            if (property.GetMethod != null && property.GetMethod.HasBody)
            {
                WeaveMethod(module, property.GetMethod, aspectAttrs);
            }

            // Apply to the Setter if it exists
            if (property.SetMethod != null && property.SetMethod.HasBody)
            {
                WeaveMethod(module, property.SetMethod, aspectAttrs);
            }
        }
    }

    private static void WeaveMethod(
        ModuleDefinition module,
        MethodDefinition method,
        List<CustomAttribute> aspects)
    {
        var body = method.Body;
        body.SimplifyMacros();
        method.DebugInformation?.SequencePoints.Clear();

        var il = body.GetILProcessor();

        // remove attributes
        foreach (var a in aspects)
            method.CustomAttributes.Remove(a);

        // only one aspect for now
        var aspectAttr = aspects[0];
        var aspectTypeRef = module.ImportReference(aspectAttr.AttributeType);

        // local: aspect
        var aspectVariableDefinition = new VariableDefinition(aspectTypeRef);
        body.Variables.Add(aspectVariableDefinition);
        body.InitLocals = true;

        // stash original instructions
        var originalInstructions = body.Instructions.ToList();

        // reset body
        body.Instructions.Clear();
        body.ExceptionHandlers.Clear();

        // import AspectNetDependencyInjection.GetRequiredService<T>()
        var diType = module.ImportReference(
            typeof(AspectNetDependencyInjection));
        var diResolved = diType.Resolve();

        var getRequiredServiceGeneric = diResolved.Methods
            .First(m => m.Name == "GetRequiredService" && m.HasGenericParameters);

        var getRequiredServiceClosed = new GenericInstanceMethod(module.ImportReference(getRequiredServiceGeneric));
        getRequiredServiceClosed.GenericArguments.Add(aspectTypeRef);

        // resolve base AspectNet attribute methods
        var onEntry = module.FindMethod(aspectTypeRef, nameof(AbstractAspectNetAttribute.OnEntry), 1);
        var onExit = module.FindMethod(aspectTypeRef, nameof(AbstractAspectNetAttribute.OnExit), 1);
        var onException = module.FindMethod(aspectTypeRef, nameof(AbstractAspectNetAttribute.OnException), 1);

        // aspect = AspectNetDependencyInjection.GetRequiredService<LogAttribute>();
        il.Append(Instruction.Create(OpCodes.Call, getRequiredServiceClosed));
        il.Append(Instruction.Create(OpCodes.Stloc, aspectVariableDefinition));

        method.WeaveWithContextAndReturn(aspectVariableDefinition, onEntry, onException, onExit, originalInstructions);

        body.OptimizeMacros();
    }
}
