using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SimpleBitware.AspectNet.Debugging;

internal static class TaskLoggingHelperExtensions
{
    public static void LogWeavingMessage(this TaskLoggingHelper log,  bool debug, string message, params object[] messageArgs)
    {
        if(debug)
            log.LogMessage(MessageImportance.High, message, messageArgs);
    }
}
