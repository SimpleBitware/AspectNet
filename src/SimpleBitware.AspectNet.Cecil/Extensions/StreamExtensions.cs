namespace SimpleBitware.AspectNet.Cecil.Extensions;

internal static class StreamExtensions
{
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
