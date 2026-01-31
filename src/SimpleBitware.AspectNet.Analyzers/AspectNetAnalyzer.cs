using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SimpleBitware.AspectNet.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AspectNetAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [DiagnosticDescriptors.ClassMustBePartial];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        context.RegisterSymbolAction(AnalyzeClass2, SymbolKind.NamedType);

    }

    private static void AnalyzeClass2(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        //
        // if (!HasAspectAttributes(type))
        //     return;
        //
        // if (!type.DeclaringSyntaxReferences.Any(s => s.GetSyntax() is ClassDeclarationSyntax cds && cds.Modifiers.Any(SyntaxKind.PartialKeyword)))
        // {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.ClassMustBePartial,
                type.Locations[0],
                type.Name);
            context.ReportDiagnostic(diagnostic);
        // }
    }
    
    private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;

        // // Only classes with your attribute need to be partial
        // if (!HasAspectAttribute(classDecl, context.SemanticModel))
        //     return;
        //
        // // Already partial → OK
        // if (classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
        //     return;

        // Report diagnostic
        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticDescriptors.ClassMustBePartial,
                classDecl.Identifier.GetLocation(),
                classDecl.Identifier.Text));
    }

    private static bool HasAspectAttribute(ClassDeclarationSyntax cls, SemanticModel model)
    {
        foreach (var attrList in cls.AttributeLists)
        foreach (var attr in attrList.Attributes)
        {
            var symbol = ModelExtensions.GetSymbolInfo(model, attr).Symbol as IMethodSymbol;
            if (symbol?.ContainingType.ToDisplayString() == "SimpleBitware.AspectNet.AspectAttribute")
                return true;
        }

        return false;
    }
}
