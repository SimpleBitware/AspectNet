using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace SimpleBitware.AspectNet.Common.Extensions;

internal static class ImmutableArrayExtensions
{
    public static Dictionary<INamedTypeSymbol, List<WeaveCandidate>> ToSymbolGroups(this ImmutableArray<(INamedTypeSymbol Key, WeaveCandidate Value)> namedTypeSymbols)
    {
        var namedTypeSymbolsGroups = new Dictionary<INamedTypeSymbol, List<WeaveCandidate>>(SymbolEqualityComparer.Default);
        foreach (var (key, value) in namedTypeSymbols)
        {
            if (!namedTypeSymbolsGroups.TryGetValue(key, out var list))
                namedTypeSymbolsGroups[key] = list = [];

            list.Add(value);
        }
        
        return namedTypeSymbolsGroups;
    }
}
