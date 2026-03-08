using Mono.Cecil;

namespace SimpleBitware.AspectNet.Extensions.Cecil;

public static class TypeDefinitionExtensions
{
    public static bool InheritsFrom(this TypeDefinition? type, TypeDefinition baseType)
    {
        var currentType = type;
        while (currentType != null)
        {
            if (currentType.FullName == baseType.FullName)
                return true;

            currentType = currentType.BaseType?.Resolve();
        }

        return false;
    }
}