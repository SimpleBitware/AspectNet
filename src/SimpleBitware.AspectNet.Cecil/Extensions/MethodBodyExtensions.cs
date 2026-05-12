using Mono.Cecil.Cil;
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
    /// <param name="processor">The IL processor used to apply the instructions.</param>
    public static void Apply(this InstructionSet instructionSet, ILProcessor processor)
    {
        instructionSet.Instructions
            .ForEach(processor.Append);
    }
}
