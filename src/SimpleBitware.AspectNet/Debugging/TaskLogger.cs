using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;

namespace SimpleBitware.AspectNet.Debugging;

/// <summary>
/// Provides logging functionality for AspectNet MSBuild tasks.
/// Filters log messages based on the configured log level and prefixes them with an AspectNet identifier.
/// </summary>
internal class TaskLogger(TaskLoggingHelper taskLoggingHelper, LogLevel logLevel)
{
    private const string MessagePrefix = "[AspectNet]";
    private readonly TaskLoggingHelper taskLoggingHelper = taskLoggingHelper ?? throw new ArgumentNullException(nameof(taskLoggingHelper));

    /// <summary>
    /// Logs a debug message if the current log level allows debug output.
    /// </summary>
    /// <param name="message">The message format string.</param>
    /// <param name="messageArgs">Format arguments for the message.</param>
    public void LogDebug(string message, params object[] messageArgs)
    {
        if(logLevel <= LogLevel.Debug)
            taskLoggingHelper.LogMessage(MessageImportance.High, $"{MessagePrefix} {message}", messageArgs);
    }
    
    /// <summary>
    /// Logs an information message if the current log level allows information output.
    /// </summary>
    /// <param name="message">The message format string.</param>
    /// <param name="messageArgs">Format arguments for the message.</param>
    public void LogInformation(string message, params object[] messageArgs)
    {
        if(logLevel <= LogLevel.Information)
            taskLoggingHelper.LogMessage(MessageImportance.High, $"{MessagePrefix} {message}", messageArgs);
    }
    
    /// <summary>
    /// Logs an error message if the current log level allows error output.
    /// </summary>
    /// <param name="message">The message format string.</param>
    /// <param name="messageArgs">Format arguments for the message.</param>
    public void LogError(string message, params object[] messageArgs)
    {
        if(logLevel <= LogLevel.Error)
            taskLoggingHelper.LogError($"{MessagePrefix} {message}", messageArgs);
    }
    
    /// <summary>
    /// Logs an exception if the current log level allows error output.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    public void LogErrorFromException(Exception exception)
    {
        if(logLevel <= LogLevel.Error)
            taskLoggingHelper.LogErrorFromException(exception, true);
    }
}
