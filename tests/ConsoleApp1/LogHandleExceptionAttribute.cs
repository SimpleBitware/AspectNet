using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Abstractions.Context;

namespace ConsoleApp1;

public class LogMeWithException : AbstractAspectNetAttribute
{
   public override void OnEntry(AspectNetEntryContext entryContext)
   {
      Console.WriteLine($"OnEntry LogMeWithException: {entryContext.ClassName}, {entryContext.MemberName}, Parameters: {string.Join(", ", entryContext.Parameters.Select(kv => $"{kv.Key}={kv.Value}"))}");
      base.OnEntry(entryContext);
   }

   public override void OnExit(AspectNetExitContext context)
   {
      context.ReturnValue = 6969;
      Console.WriteLine($"OnExit LogMeWithException: {context.ClassName}, {context.MemberName}, Parameters: {string.Join(", ", context.Parameters.Select(kv => $"{kv.Key}={kv.Value}"))}, ReturnValue: {context.ReturnValue}");
      base.OnExit(context);
   }

   public override void OnException(AspectNetExceptionContext context)
   {
      Console.WriteLine($"OnException LogMeWithException: {context.ClassName}, {context.MemberName}, Parameters: {string.Join(", ", context.Parameters.Select(kv => $"{kv.Key}={kv.Value}"))}, Exception: {context.Exception}");
      context.Exception = null;
      base.OnException(context);
   }
}  
