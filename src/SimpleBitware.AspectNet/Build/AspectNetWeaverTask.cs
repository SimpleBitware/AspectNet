using Ardalis.GuardClauses;
using Microsoft.Build.Framework;
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
            Log.LogWeavingMessage(ShowWeavingLogs, "[AspectNet] Starting weaving assembly {0}", AssemblyPath);

            Guard.Against.NullOrEmpty(AssemblyPath);
            Guard.Against.FileDoesNotExists(AssemblyPath);

            var targetAssemblyDirectory = GetTargetAssemblyDirectory(AssemblyPath);
            var pdbFilePath = GetPdbFilePath(AssemblyPath);
            var references = References
                .Select(x => x.ItemSpec)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToArray();

            var updatedFiles = CecilWeaver.ProcessAssembly(targetAssemblyDirectory, references, AssemblyPath, pdbFilePath, GenerateDebugFiles);

            LogUpdatedFiles(updatedFiles);
            Log.LogWeavingMessage(ShowWeavingLogs, "[AspectNet] Completed weaving assembly {0}", AssemblyPath);
            return true;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex, ShowWeavingLogs);
            Log.LogError("[AspectNet] Error weaving assembly {0}", AssemblyPath);
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

    private void LogUpdatedFiles(string[] files)
    {
        if (!ShowWeavingLogs)
            return;
        
        foreach (var filePath in files)
            Log.LogWeavingMessage(ShowWeavingLogs, "[AspectNet] Updated file: {0}", filePath);
    }
}
