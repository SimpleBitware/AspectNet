using System;
using System.IO;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SimpleBitware.AspectNet.Build;

public sealed class SourceWeaver : Task
{
    [Required]
    public string ProjectDir { get; set; }

    [Required]
    public string OutDir { get; set; }

    public override bool Execute()
    {
        System.Diagnostics.Debugger.Launch();

        try
        {
            var taskDir = Path.GetDirectoryName(GetType().Assembly.Location);
            var enginePath = Path.Combine(taskDir, "SimpleBitware.AspectNet.Engine.dll");

            var engineAsm = Assembly.LoadFrom(enginePath);
            var weaverType = engineAsm.GetType("SimpleBitware.AspectNet.Engine.Weaver", true);
            var weaver = Activator.CreateInstance(weaverType);

            var run = weaverType.GetMethod("Run");

            try
            {
                run.Invoke(weaver, new object[] { ProjectDir, OutDir });
            }
            catch (TargetInvocationException tie)
            {
                var inner = tie.InnerException ?? tie;
                Log.LogError($"AspectNet engine error: {inner.GetType().Name}: {inner.Message}");
                Log.LogError(inner.StackTrace);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex, true);
            return false;
        }
    }

}
