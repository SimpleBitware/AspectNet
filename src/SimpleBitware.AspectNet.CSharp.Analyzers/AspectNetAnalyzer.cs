using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SimpleBitware.AspectNet.Abstractions;
using SimpleBitware.AspectNet.Common.Diagnostics;
using SimpleBitware.AspectNet.CSharp.Analyzers.Extensions;

namespace SimpleBitware.AspectNet.CSharp.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AspectNetAnalyzer : DiagnosticAnalyzer
{
    private static readonly string? AspectNetAttributeFullName = typeof(AspectNetAttribute).FullName;
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [DiagnosticDescriptors.ClassMustBePartial];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeClass, SymbolKind.NamedType);
    }

    private static void AnalyzeClass(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;

        // Only analyze classes
        if (typeSymbol.TypeKind != TypeKind.Class)
            return;

        // Resolve the aspect attribute symbol
        var aspectAttributeSymbol = context.Compilation.GetTypeByMetadataName(AspectNetAttributeFullName);

        if (aspectAttributeSymbol is null)
            return; // attribute not referenced in this compilation

        // Check if ANY member has the aspect attribute
        var hasAspectMembers = typeSymbol
            .GetMembers()
            .Any(member => member.GetAttributes().Any(attr => attr.AttributeClass.InheritsFrom(aspectAttributeSymbol)));

        if (!hasAspectMembers)
            return;

        // If the class has aspect-decorated members, it MUST be partial
        if (!typeSymbol.DeclaringSyntaxReferences.Any(syntaxRef =>
                syntaxRef.GetSyntax() is ClassDeclarationSyntax cds &&
                cds.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))))
        {
            // Report diagnostic on the class declaration
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.ClassMustBePartial,
                    typeSymbol.Locations.FirstOrDefault(),
                    typeSymbol.Name));
        }
    }
}
