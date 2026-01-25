using System;
using System.IO;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SimpleBitware.AspectNet.Build;

public sealed class SourceWeaver : Task
{
    private const string AspectNetEngineAssemblyName = "SimpleBitware.AspectNet.Engine.dll";
    private const string WeaverTypeName = "SimpleBitware.AspectNet.Engine.Weaver";
    private const string WeaverEntryMethodName = "Run";

    [Required] 
    public string ProjectDirectory { get; set; }

    [Required] 
    public string OutputDirectory { get; set; }
    
    [Required]
    public bool DebugMode  { get; set; }

    public override bool Execute()
    {
        try
        {
            var weaverType = GetWeaverType();
            return ExecuteWeaver(weaverType);
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex, true);
            return false;
        }
    }

    private Type GetWeaverType()
    {
        var taskDir = Path.GetDirectoryName(GetType().Assembly.Location) ?? string.Empty;
        var enginePath = Path.Combine(taskDir, AspectNetEngineAssemblyName);
        var engineAssembly = Assembly.LoadFrom(enginePath);
        return engineAssembly.GetType(WeaverTypeName, true);
    }

    private bool ExecuteWeaver(Type weaverType)
    {
        var weaverEntryMethod = weaverType.GetMethod(WeaverEntryMethodName);
        if (weaverEntryMethod == null)
        {
            Log.LogError("AspectNet engine error: {1} method not found", WeaverEntryMethodName);
            return false;
        }

        try
        {
            var weaver = Activator.CreateInstance(weaverType);
            weaverEntryMethod.Invoke(weaver, [ProjectDirectory, OutputDirectory, DebugMode]);
            return true;
        }
        catch (TargetInvocationException ex)
        {
            var exception = ex.InnerException ?? ex;
            Log.LogErrorFromException(exception,  true);
            return false;
        }
    }
}
