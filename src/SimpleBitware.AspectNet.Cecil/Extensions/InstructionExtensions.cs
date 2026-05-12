using Mono.Cecil.Cil;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

public static class InstructionExtensions
{
    public static List<Instruction> ApplyPeepholeOptimization(this List<Instruction> instructions)
    {
        if (instructions.Count < 3) return instructions;
        
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
        return instructions;
    }
    
    private static int GetVariableIndex(Instruction instruction)
    {
        return instruction.OpCode.Code switch
        {
            Code.Ldloc_0 or Code.Stloc_0 => 0,
            Code.Ldloc_1 or Code.Stloc_1 => 1,
            Code.Ldloc_2 or Code.Stloc_2 => 2,
            Code.Ldloc_3 or Code.Stloc_3 => 3,
            Code.Ldloc_S or Code.Stloc_S or Code.Ldloc or Code.Stloc => (instruction.Operand as VariableDefinition)?.Index ?? -1,
            _ => -1
        };
    }
}
