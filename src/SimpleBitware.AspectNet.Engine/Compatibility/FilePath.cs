namespace SimpleBitware.AspectNet.Engine.Compatibility;

public static class FilePath
{
    public static string GetRelativePath(string basePath, string fullPath)
    {
        var baseUri = new Uri(AppendDirectorySeparator(basePath));
        var fullUri = new Uri(fullPath);

        return Uri.UnescapeDataString(baseUri.MakeRelativeUri(fullUri).ToString())
            .Replace('/', Path.DirectorySeparatorChar);
    }

    private static string AppendDirectorySeparator(string path)
    {
        return path.EndsWith(Path.DirectorySeparatorChar.ToString())
            ? path
            : path + Path.DirectorySeparatorChar;
    }
}
