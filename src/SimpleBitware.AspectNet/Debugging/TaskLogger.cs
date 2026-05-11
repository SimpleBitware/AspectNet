using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;

namespace SimpleBitware.AspectNet.Debugging;

internal class TaskLogger(TaskLoggingHelper taskLoggingHelper, LogLevel logLevel)
{
    private const string MessagePrefix = "[AspectNet]";
    private readonly TaskLoggingHelper taskLoggingHelper = taskLoggingHelper ?? throw new ArgumentNullException(nameof(taskLoggingHelper));

    public void LogDebug(string message, params object[] messageArgs)
    {
        if(logLevel <= LogLevel.Debug)
            taskLoggingHelper.LogMessage(MessageImportance.High, $"{MessagePrefix} {message}", messageArgs);
    }
    
    public void LogInformation(string message, params object[] messageArgs)
    {
        if(logLevel <= LogLevel.Information)
            taskLoggingHelper.LogMessage(MessageImportance.High, $"{MessagePrefix} {message}", messageArgs);
    }
    
    public void LogError(string message, params object[] messageArgs)
    {
        if(logLevel <= LogLevel.Error)
            taskLoggingHelper.LogError($"{MessagePrefix} {message}", messageArgs);
    }
    
    public void LogErrorFromException(Exception exception)
    {
        if(logLevel <= LogLevel.Error)
            taskLoggingHelper.LogErrorFromException(exception, true);
    }
}
