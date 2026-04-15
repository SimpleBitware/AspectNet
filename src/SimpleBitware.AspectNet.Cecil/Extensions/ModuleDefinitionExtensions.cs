using System.Collections.Concurrent;
using Mono.Cecil;
using SimpleBitware.AspectNet.Cecil.Helpers;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

internal static class ModuleDefinitionExtensions
{
    private static readonly ConcurrentDictionary<ModuleDefinition, ModuleCache> ModulesCache = new();
    public static ModuleCache Cache(this ModuleDefinition module)
    {
        ModulesCache.TryAdd(module, new ModuleCache(module));
        return ModulesCache[module];
    }
}
