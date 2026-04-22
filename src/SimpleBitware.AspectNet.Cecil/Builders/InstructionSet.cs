using Mono.Cecil.Cil;

namespace SimpleBitware.AspectNet.Cecil.Builders;

public class InstructionSet
{
    public Instruction[] Instructions { get; init; } = [];
}
