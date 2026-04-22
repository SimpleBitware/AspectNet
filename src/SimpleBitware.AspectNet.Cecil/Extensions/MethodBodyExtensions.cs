using Mono.Cecil;
using MoreLinq;
using SimpleBitware.AspectNet.Cecil.Builders;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

public static class MethodBodyExtensions
{
    public static void ApplyTo(this InstructionSet instructionSet, MethodDefinition method)
    {
        var processor =  method.Body.GetILProcessor();
        
        method.Body.Instructions.Clear();
        
        instructionSet.Instructions
            .ForEach(processor.Append);
    }
}
