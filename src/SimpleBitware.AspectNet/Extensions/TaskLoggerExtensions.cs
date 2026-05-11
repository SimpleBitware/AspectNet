using SimpleBitware.AspectNet.Cecil.Runtime;
using SimpleBitware.AspectNet.Debugging;

namespace SimpleBitware.AspectNet.Extensions;

internal static class TaskLoggerExtensions
{
    public static void Log(this TaskLogger logger, WeavingResult result)
    {
        logger?.LogInformation("{0} items have been cached during weaving process.", result.CachedItems.Length);
        foreach (var item in result.CachedItems)
        {
            logger?.LogDebug("Cached item: {0}", item);
        }
        
        if(result.AssemblyFileName is not null)
            logger?.LogInformation("Successfully updated assembly file: {0}", result.AssemblyFileName);
        
        if(result.PdbFileName is not null)
            logger?.LogInformation("Successfully updated PDB file: {0}", result.PdbFileName);
    }
}
