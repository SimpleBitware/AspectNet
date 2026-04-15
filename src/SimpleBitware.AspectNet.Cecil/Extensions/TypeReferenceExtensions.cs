using Mono.Cecil;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

public static class TypeReferenceExtensions
{
    private static readonly string? TaskFullName = typeof(Task).FullName;
    private static readonly string? ValueTaskFullName = typeof(ValueTask).FullName;
    private static readonly string? GenericTaskFullName = typeof(Task<>).FullName;
    private static readonly string? GenericValueTaskFullName = typeof(ValueTask<>).FullName;
    
    public static bool IsTaskType(this TypeReference type)
    {
        return type.IsTask() || type.IsGenericTask();
    }

    private static bool IsTask(this TypeReference type) => (type.FullName == TaskFullName || type.FullName == ValueTaskFullName);

    private static bool IsGenericTask(this TypeReference type)
    {
        if (!type.IsGenericInstance || type is not GenericInstanceType genericType) 
            return false;
        
        return genericType.ElementType.FullName == GenericTaskFullName || 
               genericType.ElementType.FullName == GenericValueTaskFullName;
    }
}
