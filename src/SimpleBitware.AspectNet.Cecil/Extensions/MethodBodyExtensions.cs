using Mono.Cecil;
using MoreLinq;
using SimpleBitware.AspectNet.Cecil.Builders;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

/// <summary>
/// Provides extension methods for working with method bodies in Mono.Cecil.
/// </summary>
/// <remarks>
/// This class contains utilities for applying instruction sets to method bodies.
/// </remarks>
public static class MethodBodyExtensions
{
    /// <summary>
    /// Applies an instruction set to a method definition, replacing its current body.
    /// </summary>
    /// <param name="instructionSet">The instruction set containing the IL instructions to apply.</param>
    /// <param name="method">The method definition whose body will be replaced.</param>
    public static void ApplyTo(this InstructionSet instructionSet, MethodDefinition method)
    {
        var processor =  method.Body.GetILProcessor();
        instructionSet.Instructions
            .ForEach(processor.Append);
    }
}
