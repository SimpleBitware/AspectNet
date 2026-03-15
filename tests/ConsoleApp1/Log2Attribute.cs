using SimpleBitware.AspectNet.Abstractions;

namespace ConsoleApp1;

public class Log2Attribute : AbstractAspectNetAttribute
{
   public override void OnEntry(AspectNetEntryContext entryContext)
   {
      Console.WriteLine($"OnEntry2: {entryContext.ClassName}, {entryContext.MemberName}, Parameters: {string.Join(", ", entryContext.Parameters.Select(kv => $"{kv.Key}={kv.Value}"))}");
      base.OnEntry(entryContext);
   }

   public override void OnExit(AspectNetExitContext context)
   {
      Console.WriteLine($"OnExit2: {context.ClassName}, {context.MemberName}, Parameters: {string.Join(", ", context.Parameters.Select(kv => $"{kv.Key}={kv.Value}"))}, ReturnValue: {context.ReturnValue}");
      base.OnExit(context);
   }

   public override void OnException(AspectNetExceptionContext context)
   {
      Console.WriteLine($"OnException2: {context.ClassName}, {context.MemberName}, Parameters: {string.Join(", ", context.Parameters.Select(kv => $"{kv.Key}={kv.Value}"))}, Exception: {context.Exception}");
      base.OnException(context);
   }
}  
