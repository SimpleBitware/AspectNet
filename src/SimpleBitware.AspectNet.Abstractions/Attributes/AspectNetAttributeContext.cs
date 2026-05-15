namespace SimpleBitware.AspectNet.Abstractions.Attributes;

/// <summary>
/// Provides context information for aspect method executions.
/// Contains details about the method being executed, its parameters, return value, and allows aspects to modify behavior.
/// </summary>
public sealed class AspectNetAttributeContext
{
    /// <summary>
    /// Gets or sets the instance of the class containing the decorated method. Null for static methods.
    /// </summary>
    public object? Instance { get; init; }
    
    /// <summary>
    /// Gets or sets the type of the class containing the decorated method.
    /// </summary>
    public required Type ClassType { get; init; }
    
    /// <summary>
    /// Gets or sets the name of the decorated method, property, or constructor.
    /// </summary>
    public required string MemberName { get; init; }
    
    /// <summary>
    /// Gets or sets a dictionary of method parameters and their values.
    /// </summary>
    public Dictionary<string, object?> Parameters { get; init; } = [];
    
    /// <summary>
    /// Gets or sets the exception thrown by the decorated method.
    /// Set to a different exception to throw a different exception than the original one.
    /// Set to null to suppress the original exception and prevent it from being thrown.
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Gets or sets the return value of the decorated method.
    /// The method returns the default value of its return type when this value is null.
    /// This property has no effect on async methods.
    /// </summary>
    public object? ReturnValue { get; set; }
    
    /// <summary>
    /// Gets or sets arbitrary data that can be shared between aspect lifecycle method calls for the same execution.
    /// Useful for passing state between OnEntry, OnSuccess/OnException, and OnExit calls.
    /// </summary>
    public object? Data { get; set; }
}
