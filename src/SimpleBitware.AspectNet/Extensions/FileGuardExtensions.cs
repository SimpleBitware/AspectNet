using Ardalis.GuardClauses;

namespace SimpleBitware.AspectNet.Extensions;

public static class FileGuardExtensions
{
    extension(IGuardClause guard)
    {
        public string FileDoesNotExists(string path)
        {
            return !File.Exists(path) ? throw new FileNotFoundException($"File not found: {path}", path) : path;
        }
    }
}

