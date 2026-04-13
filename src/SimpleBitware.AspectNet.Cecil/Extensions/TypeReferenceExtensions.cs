using Mono.Cecil;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

public static class TypeReferenceExtensions
{
    public static bool IsTaskType(this TypeReference type)
    {
        return type.IsTask() || type.IsGenericTask();
    }

    private static bool IsTask(this TypeReference type) => (type.FullName == typeof(Task).FullName || type.FullName == typeof(ValueTask).FullName);

    private static bool IsGenericTask(this TypeReference type)
    {
        if (!type.IsGenericInstance || type is not GenericInstanceType genericType) 
            return false;
        
        return genericType.ElementType.FullName == typeof(Task<>).FullName || 
               genericType.ElementType.FullName == typeof(ValueTask<>).FullName;
    }
}
