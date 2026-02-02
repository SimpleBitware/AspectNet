using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace SimpleBitware.AspectNet.Common;

public interface IWeaver
{
    IEnumerable<SourceFile> GenerateSourceFiles(ISemanticWeavingPlanner semanticWeavingPlanner, INamedTypeSymbol namedTypeSymbol, IReadOnlyList<WeaveCandidate> candidateSymbols);
}
