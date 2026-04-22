using Mono.Cecil.Cil;

namespace SimpleBitware.AspectNet.Cecil.Builders;

/// <summary>
/// Represents a set of IL instructions that can be inserted into a method body during weaving.
/// </summary>
/// <remarks>
/// This immutable class serves as the output of various builder classes, encapsulating
/// the IL instructions that have been generated through the fluent builder pattern.
/// </remarks>
public class InstructionSet
{
    /// <summary>
    /// Gets the array of IL instructions.
    /// </summary>
    /// <value>An array of <see cref="Instruction"/> objects representing the IL operations to be performed.</value>
    public Instruction[] Instructions { get; init; } = [];
}
