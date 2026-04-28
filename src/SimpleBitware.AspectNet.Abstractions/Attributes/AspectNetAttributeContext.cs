namespace SimpleBitware.AspectNet.Abstractions.Attributes;

public sealed class AspectNetAttributeContext
{
    public object? Instance { get; init; }
    public required Type ClassType { get; init; }
    public required string MemberName { get; init; }
    public Dictionary<string, object?> Parameters { get; init; } = [];
    
    /// <summary>
    /// Gets or sets the exception.
    /// Set the exception to throw a different exception than the original one.
    /// Set the exception to null to hide the original exception and prevent it to be thrown.
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// Gets or sets the return value.
    /// </summary>
    /// <remarks>
    /// The method returns the default value of the returned type when this value is null.
    /// Setting this property doesn't have any effect on async methods.
    /// </remarks>
    public object? ReturnValue { get; set; }
    
    /// <summary>
    /// Used to pass in any data between the same aspect methods calls.
    /// </summary>
    public object? Data { get; set; }
}
