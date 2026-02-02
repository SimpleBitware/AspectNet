using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SimpleBitware.AspectNet.Common;

/// <summary>
/// Takes a type and its members, call SemanticWeavingPlanner for each member and calls Emitter
/// </summary>
public class Weaver(ICodeEmitter codeEmitter) : IWeaver
{
    private readonly ICodeEmitter codeEmitter = codeEmitter ?? throw new ArgumentNullException(nameof(codeEmitter));

    public IEnumerable<SourceFile> GenerateSourceFiles(ISemanticWeavingPlanner semanticWeavingPlanner, INamedTypeSymbol namedTypeSymbol, IReadOnlyList<WeaveCandidate> candidateSymbols)
    {
        var semanticWeavingPlans = candidateSymbols
            .Select(weaveTarget => semanticWeavingPlanner.TryGenerateWeavingPlan(in weaveTarget))
            .OfType<SemanticWeavingPlan>()
            .ToImmutableArray();

        if (semanticWeavingPlans.Length == 0)
            yield break;
        
        yield return codeEmitter.Emit(namedTypeSymbol, semanticWeavingPlans);
    }
}
