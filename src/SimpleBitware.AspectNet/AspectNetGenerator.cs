using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SimpleBitware.AspectNet;

[Generator]
public sealed class AspectNetGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Discover candidate members
        var candidates = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => HasAttributes(node), 
                transform: static (ctx, _) => (Member: (MemberDeclarationSyntax)ctx.Node, Model: ctx.SemanticModel)
            )
            .Select(static (x, _) => x)
            .Where(static x => x.Member is not null)
            .Where(static x => x.Member.Modifiers.Any(SyntaxKind.PartialKeyword));

        // 2. Run your weaving pipeline
        var woven = candidates
            .Select(static (item, _) =>
            {
                var (member, model) = item;
                return WeaverPipeline.TryWeave(member, model);
            })
            .Where(static result => result is not null);

        // 3. Register the output (this is where your question points)
        context.RegisterSourceOutput(woven, Generate);
    }

    private static bool HasAttributes(SyntaxNode node) => node is MemberDeclarationSyntax { AttributeLists.Count: > 0 };
    
    private static void Generate(SourceProductionContext spc, WeaveResult? result)
    {
        if (result is null)
            return;
        
        spc.AddSource(result.HintName, result.SourceText);
    }
}

