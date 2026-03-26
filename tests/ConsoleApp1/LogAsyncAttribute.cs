using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Abstractions.Context;

namespace ConsoleApp1;

public class LogAsyncAttribute : AbstractAspectNetAttribute
{
   public override void OnEntry(AspectNetAttributeContext entryContext)
   {
      Console.WriteLine($"OnEntry Async: {entryContext.ClassType.FullName}, {entryContext.MemberName}, Parameters: {string.Join(", ", entryContext.Parameters.Select(kv => $"{kv.Key}={kv.Value}"))}");
      base.OnEntry(entryContext);
   }

   public override void OnExit(AspectNetAttributeContext context)
   {
      if (context.ReturnValue is Task originalTask)
      {
         // This ensures the original completes, then your logic runs, 
         // and a new task is returned to the caller.
         context.ReturnValue = originalTask.ContinueWith(t => 
         {
            Console.WriteLine("Original task done. Now running aspect exit logic.");
            throw new Exception("Exception from Continuation");
         });
      }
      
      Console.WriteLine($"OnExit Async: {context.ClassType.FullName}, {context.MemberName}, Parameters: {string.Join(", ", context.Parameters.Select(kv => $"{kv.Key}={kv.Value}"))}, ReturnValue: {context.ReturnValue}");
      base.OnExit(context);
   }

   public override void OnException(AspectNetAttributeContext context)
   {
      Console.WriteLine($"OnException Async: {context.ClassType.FullName}, {context.MemberName}, Parameters: {string.Join(", ", context.Parameters.Select(kv => $"{kv.Key}={kv.Value}"))}, Exception: {context.Exception}");
      base.OnException(context);
   }
}  
