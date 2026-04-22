namespace SimpleBitware.AspectNet.Cecil.Helpers;

/// <summary>
/// Provides helper methods for generating standard .NET member names.
/// </summary>
internal static class MemberNameHelper
{
    /// <summary>
    /// Generates the getter method name for a property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The getter method name in the format "get_{propertyName}".</returns>
    /// <example>
    /// <code>
    /// string getterName = MemberNameHelper.PropertyGetterName("MyProperty");
    /// // Result: "get_MyProperty"
    /// </code>
    /// </example>
    public static string PropertyGetterName(string propertyName) => $"get_{propertyName}";
    
    /// <summary>
    /// Generates the setter method name for a property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The setter method name in the format "set_{propertyName}".</returns>
    /// <example>
    /// <code>
    /// string setterName = MemberNameHelper.PropertySetterName("MyProperty");
    /// // Result: "set_MyProperty"
    /// </code>
    /// </example>
    public static string PropertySetterName(string propertyName) => $"set_{propertyName}";
}
