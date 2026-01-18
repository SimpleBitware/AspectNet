using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SimpleBitware.AspectNet.Engine;

public sealed class Weaver
{
    public void Run(string projectDir, string outDir)
    {
        Directory.CreateDirectory(outDir);

        foreach (var file in Directory.EnumerateFiles(projectDir, "*.cs", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            var tree = CSharpSyntaxTree.ParseText(text);
            var root = tree.GetRoot();

            var newText = "// Rewritten by AspectNet\n" + root.ToFullString();

            var relative = GetRelativePath(projectDir, file);
            var outPath = Path.Combine(outDir, relative);

            Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
            File.WriteAllText(outPath, newText);
        }
    }

    public static string GetRelativePath(string basePath, string fullPath)
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