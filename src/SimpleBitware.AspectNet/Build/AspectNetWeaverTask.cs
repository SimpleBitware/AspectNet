using Ardalis.GuardClauses;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using MoreLinq;
using SimpleBitware.AspectNet.Debugging;
using SimpleBitware.AspectNet.Extensions;
using SimpleBitware.AspectNet.Cecil.Runtime;
using SimpleBitware.AspectNet.Helpers;

namespace SimpleBitware.AspectNet.Build;

public class AspectNetWeaverTask : Microsoft.Build.Utilities.Task
{
    private TaskLogger? logger;
    
    [Required]
    public required string AssemblyPath { get; set; }

    [Required]
    public required ITaskItem[] References { get; set; }

    public string? LogLevel { get; set; }
    
    public bool GenerateILFiles { get; set; }

    public override bool Execute()
    {
        Initialize();
        
        try
        {
            logger?.LogInformation("Starting to weave assembly {0}", AssemblyPath);

            Guard.Against.NullOrEmpty(AssemblyPath);
            Guard.Against.FileDoesNotExists(AssemblyPath);

            var targetAssemblyDirectory = FileHelper.GetTargetAssemblyDirectory(AssemblyPath);
            var pdbFilePath = FileHelper.GetPdbFilePath(AssemblyPath);
            var references = GetReferences();
            var result = CecilWeaver.ProcessAssembly(targetAssemblyDirectory, references, AssemblyPath, pdbFilePath, GenerateILFiles);

            logger?.Log(result);
            logger?.LogInformation("Weaving completed.");
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogErrorFromException(ex);
            logger?.LogError("An error occurred while weaving assembly {0}", AssemblyPath);
            return false;
        }
    }

    private void Initialize()
    {
        if(!Enum.TryParse<LogLevel>(LogLevel, out var logLevel))
            logLevel = Microsoft.Extensions.Logging.LogLevel.Error;
        
        logger = new TaskLogger(Log, logLevel);
    }

    private string[] GetReferences()
    {
        var references = References
            .Select(x => x.ItemSpec)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToArray();

        logger?.LogInformation("Using {0} external assemblies.", references.Length);
        references
            .ForEach(x => logger?.LogDebug("External assembly: {0}", x));
        
        return references;
    }
}
