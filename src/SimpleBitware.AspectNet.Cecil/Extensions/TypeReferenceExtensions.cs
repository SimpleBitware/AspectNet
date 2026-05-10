using Mono.Cecil;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

/// <summary>
/// Provides extension methods for working with type references in Mono.Cecil.
/// </summary>
public static class TypeReferenceExtensions
{
    /// <summary>
    /// The full name of the Task type.
    /// </summary>
    private static readonly string? TaskFullName = typeof(Task).FullName;
    
    /// <summary>
    /// The full name of the ValueTask type.
    /// </summary>
    private static readonly string? ValueTaskFullName = typeof(ValueTask).FullName;
    
    /// <summary>
    /// The full name of the generic Task&lt;T&gt; type.
    /// </summary>
    private static readonly string? GenericTaskFullName = typeof(Task<>).FullName;
    
    /// <summary>
    /// The full name of the generic ValueTask&lt;T&gt; type.
    /// </summary>
    private static readonly string? GenericValueTaskFullName = typeof(ValueTask<>).FullName;
    
    /// <summary>
    /// Determines whether the type reference represents a task type (Task or ValueTask).
    /// </summary>
    /// <param name="type">The type reference to check.</param>
    /// <returns><c>true</c> if the type is Task, ValueTask, or a generic variant; otherwise, <c>false</c>.</returns>
    public static bool IsTaskType(this TypeReference type)
    {
        return type.IsTask() || type.IsGenericTask();
    }

    /// <summary>
    /// Determines whether the type reference represents a non-generic Task or ValueTask.
    /// </summary>
    /// <param name="type">The type reference to check.</param>
    /// <returns><c>true</c> if the type is Task or ValueTask; otherwise, <c>false</c>.</returns>
    private static bool IsTask(this TypeReference type) => (type.FullName == TaskFullName || type.FullName == ValueTaskFullName);

    /// <summary>
    /// Determines whether the type reference represents a generic Task&lt;T&gt; or ValueTask&lt;T&gt;.
    /// </summary>
    /// <param name="type">The type reference to check.</param>
    /// <returns><c>true</c> if the type is a generic Task or ValueTask; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method checks if the type is a generic instance.
    /// </remarks>
    private static bool IsGenericTask(this TypeReference type)
    {
        if (!type.IsGenericInstance || type is not GenericInstanceType genericType) 
            return false;
        
        return genericType.ElementType.FullName == GenericTaskFullName || 
               genericType.ElementType.FullName == GenericValueTaskFullName;
    }
}
