using Microsoft.CodeAnalysis;

namespace SimpleBitware.AspectNet.Common.Extensions;

public static class NamedTypeSymbolExtensions
{
    public static bool InheritsFromSymbol(this INamedTypeSymbol? namedTypeSymbol, ISymbol symbol)
    {
        if(namedTypeSymbol is null)
            return false;
        
        var type = namedTypeSymbol;
        while (type is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(type, symbol))
                return true;

            type = type.BaseType;
        }

        return false;
    }
}
