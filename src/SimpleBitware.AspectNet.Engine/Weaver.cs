using System.Diagnostics;

namespace SimpleBitware.AspectNet.Engine;

public sealed class Weaver
{
    private static readonly string[] ExcludedDirectories = ["bin", "obj"];
    private readonly ICodeFileWeaver[] weavers = [new CSharpFileWeaver()];

    public void Run(string projectDirectory, string outDirectory, bool debugMode)
    {
        if (debugMode)
            WaitForDebuggerToAttach();

        Directory.CreateDirectory(outDirectory);

        foreach (var fullFilePath in EnumerateProjectFiles(projectDirectory))
        {
            var fileContent = File.ReadAllText(fullFilePath);
            var fileExtension = Path.GetExtension(fullFilePath);

            var generatedFileContent = weavers
                .Select(x => x.Run(fileExtension, fileContent))
                .FirstOrDefault(x => x != null);

            if (generatedFileContent != null)
                CreateGeneratedFile(projectDirectory, outDirectory, fullFilePath, generatedFileContent);
        }
    }

    private static void WaitForDebuggerToAttach()
    {
        if (!Debugger.IsAttached)
        {
            Console.WriteLine($"Waiting for debugger. PID: {Process.GetCurrentProcess().Id}");
            while (!Debugger.IsAttached)
                Thread.Sleep(1000);
        }

        Debugger.Break();
    }

    private static IEnumerable<string> EnumerateProjectFiles(string currentDirectory)
    {
        if (ExcludedDirectories.Any(x => string.Equals(x, Path.GetFileName(currentDirectory.TrimEnd(Path.DirectorySeparatorChar)), StringComparison.InvariantCultureIgnoreCase)))
            yield break;

        foreach (var fullFilePath in Directory.EnumerateFiles(currentDirectory, "*.*", SearchOption.TopDirectoryOnly))
            yield return fullFilePath;

        foreach (var directory in Directory.EnumerateDirectories(currentDirectory))
        {
            foreach (var fullFilePath in EnumerateProjectFiles(directory))
            {
                yield return fullFilePath;
            }
        }
    }

    private static void CreateGeneratedFile(string projectDirectory, string outDirectory, string fullFilePath, string generatedFileContent)
    {
        var relative = GetRelativePath(projectDirectory, fullFilePath);
        var outPath = Path.Combine(outDirectory, relative);

        var outputFileDirectory = Path.GetDirectoryName(outPath) ?? string.Empty;
        Directory.CreateDirectory(outputFileDirectory);
        File.WriteAllText(outPath, generatedFileContent);
    }

    private static string GetRelativePath(string basePath, string fullPath)
    {
        var baseUri = new Uri(AppendDirectorySeparator(basePath));
        var fullUri = new Uri(fullPath);

        return Uri.UnescapeDataString(
            baseUri.MakeRelativeUri(fullUri).ToString()
        ).Replace('/', Path.DirectorySeparatorChar);
    }

    private static string AppendDirectorySeparator(string path)
    {
        if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            return path + Path.DirectorySeparatorChar;
        return path;
    }
}
