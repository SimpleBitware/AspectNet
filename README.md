<img src="resources/logo.png" width="110" align="right"></br></br>

# AspectNet

AspectNet brings Aspect-Oriented Programming (AOP) to any .NET project through compile‑time IL weaving. </br>
It enables the creation of cross‑cutting behaviors and ASP.NET‑style middleware pipelines that can be applied to any class, method, constructor, or property—regardless of visibility or static/instance context.

## Features

> ### Attribute‑driven aspect model
Defines aspects using custom attributes applied directly to classes, constructors, methods, or properties.

> ### Full member coverage
Works with private, protected, internal, and public members, as well as static and instance targets.

> ### Multiple attributes with deterministic ordering
Supports stacking multiple attributes on the same member. Execution order is determined first by the Priority property (ascending), then by declaration order (topmost applied attribute executes first for equal priorities).

> ### Rich execution context
Provides read‑only access to: </br>
- Declaring type and member metadata </br>
- Method parameters and their runtime values

> ### Exception handling pipeline
Captures thrown exceptions and allows aspects to inspect, handle, suppress, or replace them.

> ### Return value interception
Enables inspection and modification of return values before they reach the caller.

> ### Shared attribute state
Allows attribute instances to maintain state across their lifecycle during a single invocation.

> ### Debugger‑friendly weaving
Preserves debugging breakpoints in both user code and aspect code.

> ### Comprehensive async support
Fully supports `Task`, `Task<T>`, `ValueTask` and `ValueTask<T>`.

## How it works

AspectNet weaves aspects into the target assembly immediately after the build completes, using IL rewriting.

> ### Per‑project installation
Each project that uses AspectNet must reference the NuGet package. This ensures correct incremental builds and parallel compilation across solutions.

> ### Dependency‑injected aspects
When aspects require DI, call `UseAspectNet()` on the application’s `IServiceProvider`. This enables the woven code to resolve attribute instances through the application’s IoC container.

> ### Fallback construction
If no service provider is registered—or if a given aspect type is not registered—AspectNet falls back to using the attribute’s default constructor.

## Basic usage

1. Import `SimpleBitware.AspectNet` NuGet package into the target project.
2. Create a custom attribute implementing `IAspectNetAttribute` interface or extending `AbstractAspectNetAttribute` abstract class. Or just use the included AspectNet attributes.
3. Decorate classes and/or class members with AspectNet attributes:

```csharp
public class LogAttribute : AbstractAspectNetAttribute
{
   public override void OnEntry(AspectNetAttributeContext context)
   {
      base.OnEntry(context);
   }

   public override void OnSuccess(AspectNetAttributeContext context)
   {
      base.OnSuccess(context);
   }

   public override void OnException(AspectNetAttributeContext context)
   {
      base.OnException(context);
   }

   public override void OnExit(AspectNetAttributeContext context)
   {
      base.OnExit(context);
   }
}  

public partial class OrderService
{
    [Log(Priority = 1)]
    public async Task PlaceOrderAsync(string id) { ... }
}
```

## Debugging weaving process

One can control weaving process by changing configuration in `SimpleBitware.AspectNet.props` file. This file can be found in the local nuget repository folder, under `simplebitware.aspectnet\<version>\build` folder. </br>
Set `SkipAspectNetWeaving` to `true` to disable weaving. </br>
Set `ShowWeavingLogs` to `true` to show weaving logs in the build window. </br>
Exceptions thrown during weaving process are always logged in the build window.

## Would you like to contribute?

Discover [here](CONTRIBUTING.md) how you can contribute to a better developer experience for all.
