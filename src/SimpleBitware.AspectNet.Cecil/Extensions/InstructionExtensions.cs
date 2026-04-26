using Mono.Cecil.Cil;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

public static class InstructionExtensions
{
    public static List<Instruction> ApplyPeepholeOptimization(this List<Instruction> instructions)
    {
        if (instructions.Count >= 3)
        {
            var last = instructions[instructions.Count - 1];   // ldloc
            var prev1 = instructions[instructions.Count - 2];  // br
            var prev2 = instructions[instructions.Count - 3];  // stloc

            // Check if we have the pattern: Store to X -> Jump -> Load from X
            if (GetVariableIndex(last) != -1 && 
                GetVariableIndex(last) == GetVariableIndex(prev2) &&
                prev1.OpCode.FlowControl == FlowControl.Branch)
            {
                instructions.RemoveRange(instructions.Count - 3, 3);
            }
        }
        return instructions;
    }
    
    private static int GetVariableIndex(Instruction instruction)
    {
        switch (instruction.OpCode.Code)
        {
            case Code.Ldloc_0:
            case Code.Stloc_0: return 0;
            case Code.Ldloc_1:
            case Code.Stloc_1: return 1;
            case Code.Ldloc_2:
            case Code.Stloc_2: return 2;
            case Code.Ldloc_3:
            case Code.Stloc_3: return 3;
            case Code.Ldloc_S:
            case Code.Stloc_S:
            case Code.Ldloc:
            case Code.Stloc:
                return (instruction.Operand as VariableDefinition)?.Index ?? -1;
            default: return -1;
        }
    }
}
