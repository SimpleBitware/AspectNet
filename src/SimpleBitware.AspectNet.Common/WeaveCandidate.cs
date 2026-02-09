using Microsoft.CodeAnalysis;

namespace SimpleBitware.AspectNet.Common;

/// <summary>
/// Language-agnostic view of a member to be woven.
/// CodeWriters construct this from their own syntax model.
/// </summary>
public sealed record WeaveCandidate(
    ISymbol Symbol,
    SyntaxNode SyntaxNode,
    SemanticModel SemanticModel
);
