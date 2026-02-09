using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace SimpleBitware.AspectNet.Common;

public interface ICodeEmitter
{
    SourceFile? Emit(INamedTypeSymbol namedTypeSymbol, IReadOnlyList<SemanticWeavingPlan> semanticWeavingPlans);
}
