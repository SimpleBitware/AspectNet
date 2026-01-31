using System;
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
using Document = Microsoft.CodeAnalysis.Document;

namespace SimpleBitware.AspectNet.CodeFixes;

[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddPartialModifierCodeFixProvider))]
public sealed class AddPartialModifierCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("ASPECT001"); // your analyzer's ID

    public override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var classDecl = root.FindNode(diagnostic.Location.SourceSpan) as ClassDeclarationSyntax;
        if (classDecl == null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Add 'partial' modifier",
                ct => ApplyFixAsync(context.Document, classDecl, ct),
                nameof(AddPartialModifierCodeFixProvider)),
            diagnostic);
    }

    private static async Task<Document> ApplyFixAsync(
        Document document,
        ClassDeclarationSyntax classDecl,
        CancellationToken ct)
    {
        // If already partial, no-op
        if (classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            return document;

        var newModifiers = classDecl.Modifiers.Add(
            SyntaxFactory.Token(SyntaxKind.PartialKeyword)
                .WithTrailingTrivia(SyntaxFactory.Space));

        var newClassDecl = classDecl.WithModifiers(newModifiers);

        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if(root == null)
            return document;
        
        var newRoot = root.ReplaceNode(classDecl, newClassDecl);
        return document.WithSyntaxRoot(newRoot);
    }
}
