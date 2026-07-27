<img src="https://github.com/SimpleBitware/AspectNet/blob/main/resources/logo.png" width="110" align="right"></br></br>

# AspectNet

AspectNet brings Aspect-Oriented Programming (AOP) to any .NET project through compile‑time IL weaving. </br>
It enables the creation of cross‑cutting behaviors and ASP.NET‑style middleware pipelines that can be applied to any class, method, constructor, or property—regardless of visibility or static/instance context.

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

4. Aspect code will be weaved into the decorated method at build time.

## Documentation

More about AspectNet on [SimpleBitware](https://www.simplebitware.com/aspectnet.html) website.

## Would you like to contribute?

Discover [here](https://github.com/SimpleBitware/AspectNet/blob/main/CONTRIBUTING.md) how you can contribute to a better developer experience for all.
