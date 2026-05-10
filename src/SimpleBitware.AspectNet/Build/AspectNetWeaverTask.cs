using System.Reflection;
using Ardalis.GuardClauses;
using Microsoft.Build.Framework;
using MoreLinq;
using SimpleBitware.AspectNet.Debugging;
using SimpleBitware.AspectNet.Extensions;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Build;

public class AspectNetWeaverTask : Microsoft.Build.Utilities.Task
{
    [Required]
    public required string AssemblyPath { get; set; }

    [Required]
    public required ITaskItem[] References { get; set; }
    
    public bool ShowWeavingLogs { get; set; }
    
    public bool GenerateDebugFiles { get; set; }

    public override bool Execute()
    {
        try
        {
            Log.LogDebugMessage(ShowWeavingLogs, "Starting to weave assembly {0}", AssemblyPath);

            Guard.Against.NullOrEmpty(AssemblyPath);
            Guard.Against.FileDoesNotExists(AssemblyPath);

            var targetAssemblyDirectory = GetTargetAssemblyDirectory(AssemblyPath);
            var pdbFilePath = GetPdbFilePath(AssemblyPath);
            var references = GetReferences();
            var result = CecilWeaver.ProcessAssembly(targetAssemblyDirectory, references, AssemblyPath, pdbFilePath, GenerateDebugFiles);

            LogResult(result);
            Log.LogDebugMessage(ShowWeavingLogs, "Successfully completed weaving assembly {0}", AssemblyPath);
            return true;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex, ShowWeavingLogs);
            Log.LogErrorMessage("An error occurred while weaving assembly {0}", AssemblyPath);
            return false;
        }
    }

    private static string GetTargetAssemblyDirectory(string assemblyPath) => Path.GetDirectoryName(assemblyPath)
                                                                             ?? throw new InvalidOperationException($"Could not determine target assembly directory for path: {assemblyPath}");

    private static string? GetPdbFilePath(string assemblyPath)
    {
        var path = Path.ChangeExtension(assemblyPath, "pdb");
        return File.Exists(path) ? path : null;
    }

    private void LogResult(WeavingResult result)
    {
        if (!ShowWeavingLogs)
            return;

        Log.LogDebugMessage(ShowWeavingLogs, "{0} items have been cached during weaving process.", result.CachedItems.Length);
        foreach (var item in result.CachedItems)
        {
            Log.LogDebugMessage(ShowWeavingLogs, "Cached item: {0}", item);
        }
        
        if(result.AssemblyFileName is not null)
            Log.LogDebugMessage(ShowWeavingLogs, "Successfully updated assembly file: {0}", result.AssemblyFileName);
        
        if(result.PdbFileName is not null)
            Log.LogDebugMessage(ShowWeavingLogs, "Successfully updated PDB file: {0}", result.PdbFileName);
    }

    private string[] GetReferences()
    {
        var references = References
            .Select(x => x.ItemSpec)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToArray();

        Log.LogDebugMessage(ShowWeavingLogs, "Using {0} referenced assemblies.", references.Length);
        references
            .ForEach(x => Log.LogDebugMessage(ShowWeavingLogs, "Referenced assembly: {0}", x));
        
        return references;
    }
}
