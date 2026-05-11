using Ardalis.GuardClauses;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using MoreLinq;
using SimpleBitware.AspectNet.Debugging;
using SimpleBitware.AspectNet.Extensions;
using SimpleBitware.AspectNet.Cecil.Runtime;
using SimpleBitware.AspectNet.Helpers;

namespace SimpleBitware.AspectNet.Build;

/// <summary>
/// MSBuild task for weaving AspectNet aspects into assemblies.
/// This task is executed as part of the build process to apply aspect transformations to target assemblies.
/// </summary>
public class AspectNetWeaverTask : Microsoft.Build.Utilities.Task
{
    private TaskLogger? logger;
    
    /// <summary>
    /// Gets or sets the path to the assembly to be woven. This property is required.
    /// </summary>
    [Required]
    public required string AssemblyPath { get; set; }

    /// <summary>
    /// Gets or sets the array of reference assemblies needed for weaving. This property is required.
    /// </summary>
    [Required]
    public required ITaskItem[] References { get; set; }

    /// <summary>
    /// Gets or sets the logging level for diagnostic output. Valid values are Debug, Information, Warning, Error, None.
    /// Defaults to Error if not specified or invalid.
    /// </summary>
    public string? LogLevel { get; set; }

    /// <summary>
    /// Executes the weaving task on the specified assembly.
    /// </summary>
    /// <returns>true if weaving completed successfully; false if an error occurred.</returns>
    public override bool Execute()
    {
        if(!Enum.TryParse<LogLevel>(LogLevel, out var logLevel))
            logLevel = Microsoft.Extensions.Logging.LogLevel.Error;
        
        Initialize(logLevel);
        
        try
        {
            logger?.LogInformation("Starting to weave assembly {0}", AssemblyPath);

            Guard.Against.NullOrEmpty(AssemblyPath);
            Guard.Against.FileDoesNotExists(AssemblyPath);

            var targetAssemblyDirectory = FileHelper.GetTargetAssemblyDirectory(AssemblyPath);
            var pdbFilePath = FileHelper.GetPdbFilePath(AssemblyPath);
            var references = GetReferences();
            var generateDebugFiles = logLevel == Microsoft.Extensions.Logging.LogLevel.Trace;
            var result = CecilWeaver.ProcessAssembly(targetAssemblyDirectory, references, AssemblyPath, pdbFilePath, generateDebugFiles);

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

    private void Initialize(LogLevel logLevel)
    {
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
