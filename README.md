<img src="logo.png" width="110" align="right"></br></br>

# AspectNet

Enables Aspect Oriented Programming in the target project

## Features

- Attribute-driven aspects
- Supports attribute usage on classes, constructors, methods and properties, irrespective of their visibility (private, protected, public), static or instance
- Supports multiple attributes on the same class or class member and organizes execution based on their Priority (attribute property) and their position. The attributes are executed in order, lower Priority value first and, for the same Priority, first (top) applied attributes first.
- Provides access to member details (class name, member name, member parameters and their values) via a read-only context
- Provides access to exception in case one is thrown and allows to inspect it, handle it or throw a new exception
- Provides access to return value and allows to inspect it or change it
- Honors debugging breakpoints in the original code and attributes
- Async support (`Task`, `Task<T>`, `ValueTask`, `ValueTask<T>`)

## How it works

The aspects are weaven into the target assembly immediately after it is built using IL weaving. </br>
Each project using AspectNet needs to import the nuget package separately to have attributes weaven into the decorated class member. This helps with incremental and paralel projects building. </br>
DI-resolved aspects via `IServiceProvider` requires usage of `UseAspectNet()` extension method on service provider. This gives weaven code access to the application IoC to resolve potential attributes/aspects dependencies. </br>
When no service provider registered or aspect/attribute not registered with service provider, the default aspect/attribute constructor is used.

## Basic usage

1. Import `SimpleBitware.AspectNet` NuGet package into the target project.
2. Create a custom attribute implementing `IAspectNetAttribute` interface or extending `AbstractAspectNetAttribute` abstract class. Or just use the included AspectNet attributes.
3. Decorate classes and/or class members with AspectNet attributes:

```csharp
public class LogAttribute : AbstractAspectNetAttribute
{
   public override void OnEntry(AspectNetEntryContext entryContext)
   {
      base.OnEntry(entryContext);
   }

   public override void OnExit(AspectNetExitContext context)
   {
      base.OnExit(context);
   }

   public override void OnException(AspectNetExceptionContext context)
   {
      base.OnException(context);
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
