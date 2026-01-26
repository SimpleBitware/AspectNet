using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SimpleBitware.AspectNet;

public static class WeaverPipeline
{
    public static WeaveResult? TryWeave(MemberDeclarationSyntax member, SemanticModel model)
    {
        if (!AttributeInspector.HasAspectNetAttribute(member, model))
            return null;

        var rewritten = new CSharpCodeRewriter(model).Visit(member);

        return new WeaveResult(
            HintName: $"{member.GetHashCode()}.g.cs",
            SourceText: WrapInPartial(member, rewritten!)
        );
    }
    
    private static SourceText WrapInPartial(
        MemberDeclarationSyntax original,
        SyntaxNode rewritten)
    {
        var parent = (TypeDeclarationSyntax)original.Parent!;
        var ns = parent.FirstAncestorOrSelf<NamespaceDeclarationSyntax>()?.Name.ToString();

        var code = $@"
namespace {ns}
{{
    partial class {parent.Identifier}
    {{
        {rewritten.ToFullString()}
    }}
}}";

        return SourceText.From(code, Encoding.UTF8);
    }

}

public record WeaveResult
{
    public string HintName { get; set; }
    public SourceText SourceText { get; set; }
    
    public WeaveResult(string HintName, SourceText SourceText)
    {
        this.HintName = HintName;
        this.SourceText = SourceText;
    }
}

