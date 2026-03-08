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
            
            AspectNetWeaver.Run(AssemblyPath, Log);
            return true;
        }
        catch (Exception ex)
        {
            Log.LogError($"[AspectNet] {ex}");
            return false;
        }
    }
}
