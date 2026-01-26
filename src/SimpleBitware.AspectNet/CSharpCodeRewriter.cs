using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SimpleBitware.AspectNet;

public sealed class CSharpCodeRewriter : CSharpSyntaxRewriter
{
    private readonly SemanticModel _model;

    public CSharpCodeRewriter(SemanticModel model)
    {
        _model = model;
    }

    // your weaving logic here
}

