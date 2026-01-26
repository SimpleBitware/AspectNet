using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SimpleBitware.AspectNet;

[Generator]
public sealed class AspectNetGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var candidates = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => HasAttributes(node),
                static (ctx, _) => (Member: (MemberDeclarationSyntax)ctx.Node, Model: ctx.SemanticModel)
            )
            .Where(static x => x.Member is not null);

        var woven = candidates.Select(static (item, _) =>
            {
                var (member, model) = item;
                return WeaverPipeline.TryWeave(member, model);
            })
            .Where(static result => result is not null);

        context.RegisterSourceOutput(woven, static (spc, result) =>
        {
            spc.AddSource(result!.HintName, result.SourceText);
        });
    }

    private static bool HasAttributes(SyntaxNode node)
        => node is MemberDeclarationSyntax m && m.AttributeLists.Count > 0;
}

