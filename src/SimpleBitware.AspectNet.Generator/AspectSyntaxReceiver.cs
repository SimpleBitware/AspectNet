using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SimpleBitware.AspectNet.Generator;

internal sealed class AspectSyntaxReceiver : ISyntaxReceiver
{
    public List<MethodDeclarationSyntax> Candidates { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode node)
    {
        if (node is MethodDeclarationSyntax m && m.AttributeLists.Count > 0)
            Candidates.Add(m);
    }
}