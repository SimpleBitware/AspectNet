namespace SimpleBitware.AspectNet.Cecil.Helpers;

internal static class MemberNameHelper
{
    public static string PropertyGetterName(string propertyName) => $"get_{propertyName}";
    public static string PropertySetterName(string propertyName) => $"set_{propertyName}";
}
