using Microsoft.CodeAnalysis;

namespace SimpleBitware.AspectNet.CSharp.Analyzers.Extensions;

internal static class NamedTypeSymbolExtensions
{
    public static bool InheritsFrom(this INamedTypeSymbol? type, INamedTypeSymbol baseType)
    {
        while (type != null)
        {
            if (SymbolEqualityComparer.Default.Equals(type, baseType))
                return true;

            type = type.BaseType;
        }

        return false;
    }
}
