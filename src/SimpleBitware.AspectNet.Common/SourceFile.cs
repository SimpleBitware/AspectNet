using Microsoft.CodeAnalysis.Text;

namespace SimpleBitware.AspectNet.Common;

public sealed record SourceFile(
    string FileName,
    SourceText SourceText);
