namespace SimpleBitware.AspectNet.Extensions;

public static class StreamExtensions
{
    public static void SaveToFile(this Stream stream, string? filePath)
    {
        if (filePath == null)
            return;
        
        stream.Position = 0;
        using var fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write);
        stream.CopyTo(fileStream);
        
        Console.WriteLine($"File {filePath} saved."); 
    }
}
