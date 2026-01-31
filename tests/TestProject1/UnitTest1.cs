using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using SimpleBitware.AspectNet.Analyzers;
using SimpleBitware.AspectNet.CodeFixes;

namespace TestProject1;

using NUnit.Framework;

public class PartialClassTests
{
    [Test]
    public async Task AddsPartialModifier()
    {
        //var context = new CSharpCodeFixTest<AspectNetAnalyzer, AspectNetCodeFixProvider, DefaultVerifier>();
        var before = @"
using SimpleBitware.AspectNet;

[Aspect]
class Foo { }
";

        var after = @"
using SimpleBitware.AspectNet;

[Aspect]
partial class Foo { }
";

        await new VerifyCS.Test
        {
            TestCode = before,
            FixedCode = after
        }.RunAsync();
    }
}
