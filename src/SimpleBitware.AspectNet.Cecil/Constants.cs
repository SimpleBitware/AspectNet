namespace SimpleBitware.AspectNet.Cecil;

/// <summary>
/// Provides constant values used throughout the AspectNet Cecil weaving process.
/// </summary>
/// <remarks>
/// This class contains string constants that represent standard .NET naming conventions
/// and method names used during IL weaving operations.
/// </remarks>
internal static class Constants
{
    /// <summary>
    /// The standard parameter name used for property setter methods.
    /// </summary>
    /// <remarks>
    /// In .NET, property setter methods have a single parameter named "value"
    /// that contains the value being assigned to the property.
    /// </remarks>
    public const string PropertySetterParameterName = "value";
    
    /// <summary>
    /// The name of instance constructor methods in .NET.
    /// </summary>
    public const string InstanceConstructorMethodName = ".ctor";
    
    /// <summary>
    /// The name of static constructor methods in .NET.
    /// </summary>
    public const string StaticConstructorMethodName = ".cctor";
}
