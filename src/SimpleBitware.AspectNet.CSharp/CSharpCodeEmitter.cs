using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SimpleBitware.AspectNet.Common;

namespace SimpleBitware.AspectNet.CSharp;

public class CSharpCodeEmitter : ICodeEmitter
{
    public SourceFile Emit(INamedTypeSymbol namedTypeSymbol, IReadOnlyList<SemanticWeavingPlan> semanticWeavingPlans)
    {
        var fileName = GetGeneratedFileName(namedTypeSymbol.Name);
        var sourceText = EmitPartialClass(namedTypeSymbol, semanticWeavingPlans);
        
        return new SourceFile(fileName, sourceText);
    }

    private static string GetGeneratedFileName(string className)
    {
        return $"{className}.g.cs";
    }
    
    private static SourceText EmitPartialClass(INamedTypeSymbol namedTypeSymbol, IReadOnlyList<SemanticWeavingPlan> semanticWeavingPlans)
    {
        var sb = new StringBuilder(4096);

        // Namespace
        if (!namedTypeSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            sb.Append("namespace ")
              .Append(namedTypeSymbol.ContainingNamespace.ToDisplayString())
              .AppendLine(";");
            sb.AppendLine();
        }

        // Type header
        sb.Append("partial class ").Append(namedTypeSymbol.Name).AppendLine();
        sb.AppendLine("{");

        foreach (var result in semanticWeavingPlans)
        {
            EmitMember(sb, result);
            sb.AppendLine();
        }

        sb.AppendLine("}");
        
        return SourceText.From(sb.ToString(), Encoding.UTF8);
    }

    private static void EmitMember(StringBuilder sb, SemanticWeavingPlan semanticWeavingPlan)
    {
        var symbol = semanticWeavingPlan.Candidate.Symbol;

        switch (symbol)
        {
            case IMethodSymbol methodSymbol:
                EmitMethod(sb, methodSymbol, semanticWeavingPlan);
                break;

            case IPropertySymbol propertySymbol:
                EmitProperty(sb, propertySymbol, semanticWeavingPlan);
                break;

            case IFieldSymbol fieldSymbol:
                EmitField(sb, fieldSymbol, semanticWeavingPlan);
                break;

            case IEventSymbol eventSymbol:
                EmitEvent(sb, eventSymbol, semanticWeavingPlan);
                break;

            default:
                // Unsupported member type — skip
                break;
        }
    }

    private static void EmitMethod(
        StringBuilder sb,
        IMethodSymbol method,
        SemanticWeavingPlan semanticWeavingPlan)
    {
        // Basic signature
        sb.Append("    partial void ")
          .Append(method.Name)
          .Append("(");

        for (int i = 0; i < method.Parameters.Length; i++)
        {
            var p = method.Parameters[i];
            if (i > 0) sb.Append(", ");
            sb.Append(p.Type.ToDisplayString()).Append(" ").Append(p.Name);
        }

        sb.AppendLine(")");
        sb.AppendLine("    {");

        // Aspect calls (placeholder)
        foreach (var aspect in semanticWeavingPlan.Aspects)
        {
            sb.Append("        // Aspect: ")
              .AppendLine(aspect.Attribute.AttributeClass?.Name);
        }

        sb.AppendLine("        // TODO: inject OnEntry/OnExit/OnException");
        sb.AppendLine("    }");
    }

    private static void EmitProperty(
        StringBuilder sb,
        IPropertySymbol prop,
        SemanticWeavingPlan semanticWeavingPlan)
    {
        sb.Append("    // Property weaving not implemented yet: ")
          .AppendLine(prop.Name);
    }

    private static void EmitField(
        StringBuilder sb,
        IFieldSymbol field,
        SemanticWeavingPlan semanticWeavingPlan)
    {
        sb.Append("    // Field weaving not implemented yet: ")
          .AppendLine(field.Name);
    }

    private static void EmitEvent(
        StringBuilder sb,
        IEventSymbol evt,
        SemanticWeavingPlan semanticWeavingPlan)
    {
        sb.Append("    // Event weaving not implemented yet: ")
          .AppendLine(evt.Name);
    }
}
