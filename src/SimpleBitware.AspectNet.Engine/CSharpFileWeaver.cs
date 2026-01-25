using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SimpleBitware.AspectNet.Engine;

public class CSharpFileWeaver : ICodeFileWeaver
{
    public string? Run(string fileExtension, string fileContent)
    {
        if (!string.Equals(fileExtension, ".cs", StringComparison.InvariantCultureIgnoreCase))
            return null;
        
        var tree = CSharpSyntaxTree.ParseText(fileContent);
        var root = (CompilationUnitSyntax)tree.GetRoot();
        
        if (root.Members.Count == 0 ||          // If there are no members at all, it's almost certainly not C#
            tree.GetDiagnostics()               // If there are fatal syntax errors, treat as non-C#
                .Any(d => d.Severity == DiagnosticSeverity.Error))
            return null;

        return "/* weaved file */";
    }
}
