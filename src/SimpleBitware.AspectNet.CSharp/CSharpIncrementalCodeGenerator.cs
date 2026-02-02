using Microsoft.CodeAnalysis;
using SimpleBitware.AspectNet.Common;

namespace SimpleBitware.AspectNet.CSharp;

[Generator]
public sealed class CSharpIncrementalCodeGenerator() : IncrementalCodeGenerator(new Weaver(new CSharpCodeEmitter()));
