using Ardalis.GuardClauses;

namespace SimpleBitware.AspectNet.Extensions;

public static class FileGuardExtensions
{
    public static string FileDoesNotExists(this IGuardClause guard, string path)
    {
        return !File.Exists(path) ? throw new FileNotFoundException($"File not found: {path}", path) : path;
    }
}

