using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using SimpleBitware.AspectNet.Abstractions;
using SimpleBitware.AspectNet.Common.Extensions;

namespace SimpleBitware.AspectNet.Common;

/// <summary>
/// Creates complete weaving plan for a single member.
/// </summary>
public class SemanticWeavingPlanner(Compilation compilation) : ISemanticWeavingPlanner
{
    private readonly ISymbol aspectNetAttributeSymbol = compilation.GetTypeByMetadataName(typeof(AspectNetAttribute).FullName!) ?? throw new ArgumentException("Compilation doesn't contain AspectNetAttribute.", nameof(compilation));

    /// <summary>
    /// Entry point for all languages. Backends pass a fully-resolved target.
    /// Returns null if no weaving is required for this member.
    /// </summary>
    public SemanticWeavingPlan? TryGenerateWeavingPlan(in WeaveCandidate candidate)
    {
        var aspects = CollectAspects(candidate.Symbol);
        if (aspects.Count == 0)
            return null;

        if (!IsSupportedMember(candidate.Symbol))
            return null;

        var orderedAspects = OrderAspects(aspects);

        return new SemanticWeavingPlan(
            Candidate: candidate,
            Aspects: orderedAspects);
    }

    private static bool IsSupportedMember(ISymbol symbol)
    {
        // can expand this over time:
        // - disallow abstract members
        // - disallow interface members
        // - disallow extern/partial methods without body
        // - restrict to methods/properties/ctors initially

        return symbol.Kind switch
        {
            SymbolKind.Method        => true,
            SymbolKind.Property      => true,
            SymbolKind.Field         => true,
            SymbolKind.Event         => true,
            SymbolKind.NamedType     => false, // no type-level weaving yet
            _                        => false
        };
    }

    private List<AspectInstance> CollectAspects(ISymbol symbol)
    {
        return symbol
            .GetAttributes()
            .Where(a => a.AttributeClass is not null)
            .Where(a => a.AttributeClass.InheritsFromSymbol(aspectNetAttributeSymbol))
            .Select(a => new AspectInstance(a))
            .ToList();
    }

    private static IReadOnlyList<AspectInstance> OrderAspects(List<AspectInstance> aspects)
    {
        return aspects
            .OrderBy(a => a.GetOrder())
            .ThenBy(a => a.Attribute.AttributeClass?.Name, StringComparer.Ordinal)
            .ToList();
    }
}
