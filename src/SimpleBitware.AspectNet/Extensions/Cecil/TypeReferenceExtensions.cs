using Mono.Cecil;

namespace SimpleBitware.AspectNet.Extensions.Cecil;

public static class TypeReferenceExtensions
{
    public static bool IsTaskType(this TypeReference type)
    {
        string fullName = type.FullName;
    
        // 1. Direct match for non-generic Task and ValueTask
        if (fullName == "System.Threading.Tasks.Task" || 
            fullName == "System.Threading.Tasks.ValueTask")
        {
            return true;
        }

        // 2. Check for generic Task<T> or ValueTask<T>
        if (type.IsGenericInstance && type is GenericInstanceType genericType)
        {
            string baseName = genericType.ElementType.FullName;
            if (baseName == "System.Threading.Tasks.Task`1" || 
                baseName == "System.Threading.Tasks.ValueTask`1")
            {
                return true;
            }
        }

        return false;
    }
}
