
using Microsoft.CodeAnalysis.CSharp.Testing;
using SimpleBitware.AspectNet.Analyzers;
using Microsoft.CodeAnalysis.Testing;

using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    SimpleBitware.AspectNet.Analyzers.AspectNetAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace TestProject1;

public static class VerifyCS
{
    public class Test : CSharpCodeFixTest<
        SimpleBitware.AspectNet.Analyzers.AspectNetAnalyzer,
        SimpleBitware.AspectNet.CodeFixes.AspectNetCodeFixProvider,
        DefaultVerifier>
    {
    }
}

