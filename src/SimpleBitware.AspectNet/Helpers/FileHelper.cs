namespace SimpleBitware.AspectNet.Helpers;

internal static class FileHelper
{
    public static string GetTargetAssemblyDirectory(string assemblyPath) => Path.GetDirectoryName(assemblyPath)
                                                                             ?? throw new InvalidOperationException($"Could not determine target assembly directory for path: {assemblyPath}");

    public static string? GetPdbFilePath(string assemblyPath)
    {
        var path = Path.ChangeExtension(assemblyPath, "pdb");
        return File.Exists(path) ? path : null;
    }
}
