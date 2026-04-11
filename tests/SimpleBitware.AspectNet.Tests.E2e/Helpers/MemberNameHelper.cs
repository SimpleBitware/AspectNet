namespace SimpleBitware.AspectNet.Tests.E2e.Helpers;

public static class MemberNameHelper
{
    public static string PropertyGetterName(string propertyName) => $"get_{propertyName}";
    public static string PropertySetterName(string propertyName) => $"set_{propertyName}";
}
