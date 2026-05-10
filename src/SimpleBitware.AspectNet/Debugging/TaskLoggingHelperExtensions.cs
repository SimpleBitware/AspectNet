using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SimpleBitware.AspectNet.Debugging;

internal static class TaskLoggingHelperExtensions
{
    private const string MessagePrefix = "[AspectNet]";
    
    public static void LogDebugMessage(this TaskLoggingHelper log, bool debug, string message, params object[] messageArgs)
    {
        if (debug)
            log.LogMessage(MessageImportance.High, $"{MessagePrefix} {message}", messageArgs);
    }

    public static void LogErrorMessage(this TaskLoggingHelper log, string message, params object[] messageArgs)
    {
        log.LogError($"{MessagePrefix} {message}", messageArgs);
    }
}
