using Microsoft.Build.Framework;
using SimpleBitware.AspectNet.Runtime;

namespace SimpleBitware.AspectNet.Build;

public class AspectNetWeaverTask : Microsoft.Build.Utilities.Task
{
    public string? AssemblyPath { get; set; }

    public override bool Execute()
    {
        try
        {
            if(AssemblyPath == null)
                throw new ArgumentNullException(nameof(AssemblyPath));
            
            Log.LogMessage(MessageImportance.High, "[AspectNet] Starting weaving assembly {0}", AssemblyPath);
            
            var filesUpdated = AspectNetWeaver.Run(AssemblyPath);
            foreach(var filePath in filesUpdated)
                Log.LogMessage(MessageImportance.Normal, "[AspectNet] Updated file: {0}", filePath);
            
            Log.LogMessage(MessageImportance.High, "[AspectNet] Completed weaving assembly {0}", AssemblyPath);
            return true;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex);
            return false;
        }
    }
}
