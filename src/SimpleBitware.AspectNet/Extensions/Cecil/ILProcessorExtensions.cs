using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SimpleBitware.AspectNet.Extensions.Cecil;

public static class ILProcessorExtensions
{
    public static void PushValue(this ILProcessor processor, object value, TypeReference type)
    {
        switch (value)
        {
            case string s:
                processor.Append(processor.Create(OpCodes.Ldstr, s));
                break;
            case int i:
                processor.Append(processor.Create(OpCodes.Ldc_I4, i));
                break;
            case bool b:
                processor.Append(processor.Create(b ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0));
                break;
            case null:
                processor.Append(processor.Create(OpCodes.Ldnull));
                break;
        }
        // Add more types (double, float, etc.) as needed
    }
}
