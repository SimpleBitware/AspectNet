using System;
using Microsoft.CodeAnalysis;

namespace SimpleBitware.AspectNet.Common.Extensions;

internal static class CompilationExtensions
{
    public static INamedTypeSymbol GetNamedTypeSymbolOfType<T>(this Compilation compilation)
    {
        return compilation.GetTypeByMetadataName(typeof(T).FullName!) 
               ?? throw new ArgumentException("AspectNetAttribute not found.", nameof(compilation));
    }
}
