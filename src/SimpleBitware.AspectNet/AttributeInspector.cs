using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SimpleBitware.AspectNet;

public static class AttributeInspector
{
    public static bool HasAspectNetAttribute(MemberDeclarationSyntax member, SemanticModel model)
    {
        foreach (var list in member.AttributeLists)
        foreach (var attr in list.Attributes)
        {
            var type = model.GetTypeInfo(attr).Type;
            if (type is null)
                continue;

            if (InheritsFrom(type, typeof(AspectNetAttribute).FullName))
                return true;
        }

        return false;
    }

    private static bool InheritsFrom(ITypeSymbol type, string fullName)
    {
        while (type != null)
        {
            if (type.ToDisplayString() == fullName)
                return true;

            type = type.BaseType;
        }

        return false;
    }
}

