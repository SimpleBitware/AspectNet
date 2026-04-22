namespace SimpleBitware.AspectNet.Cecil.Extensions;

/// <summary>
/// Provides extension methods for working with streams.
/// </summary>
internal static class StreamExtensions
{
    /// <summary>
    /// Saves the contents of a stream to a file at the specified path.
    /// </summary>
    /// <param name="stream">The stream to save.</param>
    /// <param name="filePath">The file path to save to, or null to skip the operation.</param>
    /// <returns>The file path if the operation was successful, or null if filePath was null.</returns>
    /// <remarks>
    /// This method resets the stream position to the beginning before copying.
    /// If filePath is null, the method returns null without performing any operation.
    /// The method creates the file if it doesn't exist and overwrites it if it does.
    /// </remarks>
    public static string? SaveToFile(this Stream stream, string? filePath)
    {
        if (filePath == null)
            return null;
        
        stream.Position = 0;
        using var fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write);
        stream.CopyTo(fileStream);

        return filePath;
    }
}
