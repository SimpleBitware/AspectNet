using System.Runtime.CompilerServices;
using Mono.Cecil;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

internal static class ModuleDefinitionExtensions
{
    private static readonly ConditionalWeakTable<ModuleDefinition, ModuleCache> ModulesCache = new();
    
    public static ModuleCache Cache(this ModuleDefinition module)
    {
        return ModulesCache.GetValue(module, m => new ModuleCache(m));
    }
}
