using System.Runtime.CompilerServices;
using Mono.Cecil;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

/// <summary>
/// Provides extension methods for module definitions with caching capabilities.
/// </summary>
/// <remarks>
/// This class implements a caching mechanism for module caches using ConditionalWeakTable
/// to avoid memory leaks while providing efficient access to module-specific data.
/// </remarks>
internal static class ModuleDefinitionExtensions
{
    /// <summary>
    /// The conditional weak table that caches module caches for each module definition.
    /// </summary>
    /// <remarks>
    /// Using ConditionalWeakTable ensures that the cache entries are automatically removed
    /// when the module definition is garbage collected, preventing memory leaks.
    /// </remarks>
    private static readonly ConditionalWeakTable<ModuleDefinition, ModuleCache> ModulesCache = new();
    
    /// <summary>
    /// Gets or creates a cached module cache for the specified module definition.
    /// </summary>
    /// <param name="module">The module definition to get the cache for.</param>
    /// <returns>The cached module cache instance.</returns>
    /// <remarks>
    /// This method provides thread-safe access to module caches. If a cache doesn't exist
    /// for the module, a new one is created and stored in the cache table.
    /// </remarks>
    public static ModuleCache Cache(this ModuleDefinition module)
    {
        return ModulesCache.GetValue(module, m => new ModuleCache(m));
    }
    
    public static MethodReference ImportTaskFromResult(this ModuleDefinition module, TypeReference innerT)
    {
        // 1. Find the Task class (usually in System.Runtime or mscorlib)
        // We search for Task instead of Task`1
        var taskType = module.ImportReference(typeof(System.Threading.Tasks.Task)).Resolve();
    
        if (taskType == null)
            throw new Exception("Could not resolve System.Threading.Tasks.Task");

        // 2. Find the static FromResult<T> method
        var fromResultDef = taskType.Methods
            .FirstOrDefault(m => m.Name == "FromResult" && m.HasGenericParameters);

        if (fromResultDef == null)
            throw new Exception("Could not find Task.FromResult<T> method");

        // 3. Import the method reference into the current module
        var fromResultRef = module.ImportReference(fromResultDef);

        // 4. Make it a Generic Instance Method: Task.FromResult<innerT>
        var genericFromResult = new GenericInstanceMethod(fromResultRef);
        genericFromResult.GenericArguments.Add(innerT);

        return genericFromResult;
    }
}
