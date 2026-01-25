using SimpleBitware.AspectNet.Engine.Compatibility;
using SimpleBitware.AspectNet.Engine.Debug;

namespace SimpleBitware.AspectNet.Engine;

public sealed class Weaver
{
    private static readonly string[] ExcludedDirectories = ["bin", "obj"];
    private readonly ICodeFileWeaver[] weavers = [new CSharpFileWeaver()];

    public void Run(string projectDirectory, string outDirectory, bool debugMode)
    {
        if (debugMode)
            Debugging.WaitForDebuggerToAttach();

        Directory.CreateDirectory(outDirectory);

        foreach (var fullFilePath in EnumerateProjectFiles(projectDirectory))
        {
            var fileContent = File.ReadAllText(fullFilePath);
            var fileExtension = Path.GetExtension(fullFilePath);

            var generatedFileContent = weavers
                .Select(x => x.Run(fileExtension, fileContent))
                .FirstOrDefault(x => x != null);

            if (generatedFileContent is null)
                Console.WriteLine("Generated file for {0} is empty so it wasn't saved.", fullFilePath);
            else
            {
                var generatedFile = WriteGeneratedFile(projectDirectory, outDirectory, fullFilePath, generatedFileContent);
                Console.WriteLine("Generated file for {0} is {1}.", fullFilePath, generatedFile);
            }
        }
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

    private static string WriteGeneratedFile(string projectDirectory, string outDirectory, string fullFilePath, string generatedFileContent)
    {
        var relative = FilePath.GetRelativePath(projectDirectory, fullFilePath);
        var outPath = Path.Combine(outDirectory, relative);

        var outputFileDirectory = Path.GetDirectoryName(outPath) ?? string.Empty;
        Directory.CreateDirectory(outputFileDirectory);
        File.WriteAllText(outPath, generatedFileContent);
        
        return outPath;
    }
}
