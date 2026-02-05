# SimpleBitware.AspectNet

Compile-time aspect oriented programming for .NET using Roslyn source generators and DI-based pipelines.

## Features

- Attribute-driven aspects
- Compile-time generated proxies (no runtime reflection or dynamic proxies)
- Async support (`Task`, `Task<T>`, `ValueTask`, `ValueTask<T>`)
- DI-resolved aspects via `IServiceProvider`
- Stable method IDs for pipelines and diagnostics

## Basic usage

1. Reference the `SimpleBitware.AspectNet` NuGet package.
2. Add attributes to your methods:

```csharp
public interface IOrderService
{
    Task PlaceOrderAsync(string id);
}

public partial class OrderService : IOrderService
{
    [Log("Orders")]
    public async Task PlaceOrderAsync(string id) { ... }
}


Add the followings in the target project to save the geerated files on disk
<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
<CompilerGeneratedFilesOutputPath>obj\Debug\.generated</CompilerGeneratedFilesOutputPath>