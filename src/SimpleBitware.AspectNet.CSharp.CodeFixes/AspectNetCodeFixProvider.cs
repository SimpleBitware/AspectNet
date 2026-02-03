using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SimpleBitware.AspectNet.Common.Diagnostics;
using Document = Microsoft.CodeAnalysis.Document;

namespace SimpleBitware.AspectNet.CSharp.CodeFixes;

[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PartialModifierCodeFixProvider))]
public sealed class PartialModifierCodeFixProvider : CodeFixProvider
{
    private const string CodeActionTitle = "Add 'partial' modifier to enable AspectNet weaving";
    
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticDescriptors.ClassMustBePartial.Id];
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root?.FindNode(diagnostic.Location.SourceSpan) is not ClassDeclarationSyntax classDeclaration)
            return;

        var codeAction = CodeAction.Create(
            CodeActionTitle,
            cancellationToken => ApplyFixAsync(context.Document, classDeclaration, cancellationToken),
            nameof(PartialModifierCodeFixProvider));
        context.RegisterCodeFix(codeAction, diagnostic);
    }

    private static async Task<Document> ApplyFixAsync(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        CancellationToken cancellationToken)
    {
        if (classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            return document;

        var newModifiers = classDeclaration.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.PartialKeyword).WithTrailingTrivia(SyntaxFactory.Space));
        var newClassDeclaration = classDeclaration.WithModifiers(newModifiers);

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if(root == null)
            return document;
        
        var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);
        return document.WithSyntaxRoot(newRoot);
    }
}
