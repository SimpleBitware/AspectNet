using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Abstractions.Context;

namespace ConsoleApp1;

public class Log2Attribute : AbstractAspectNetAttribute
{
   public override void OnEntry(AspectNetAttributeContext entryContext)
   {
      Console.WriteLine($"OnEntry2: {entryContext.ClassType.FullName}, {entryContext.MemberName}, Parameters: {string.Join(", ", entryContext.Parameters.Select(kv => $"{kv.Key}={kv.Value}"))}");
      base.OnEntry(entryContext);
   }

   public override void OnExit(AspectNetAttributeContext context)
   {
      context.ReturnValue = 69;
      Console.WriteLine($"OnExit2: {context.ClassType.FullName}, {context.MemberName}, Parameters: {string.Join(", ", context.Parameters.Select(kv => $"{kv.Key}={kv.Value}"))}, ReturnValue: {context.ReturnValue}");
      base.OnExit(context);
   }

   public override void OnException(AspectNetAttributeContext context)
   {
      Console.WriteLine($"OnException2: {context.ClassType.FullName}, {context.MemberName}, Parameters: {string.Join(", ", context.Parameters.Select(kv => $"{kv.Key}={kv.Value}"))}, Exception: {context.Exception}");
      base.OnException(context);
   }
}  
