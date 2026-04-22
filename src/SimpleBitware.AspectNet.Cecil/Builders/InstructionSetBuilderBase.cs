using Mono.Cecil;
using Mono.Cecil.Cil;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Builders;

public abstract class InstructionSetBuilderBase<TBuilder>(MethodDefinition method, ILProcessor processor, ModuleCache moduleCache)
    where TBuilder : InstructionSetBuilderBase<TBuilder>
{
    protected readonly MethodDefinition Method = method ?? throw new ArgumentNullException(nameof(method));
    protected readonly ModuleCache ModuleCache = moduleCache ?? throw new ArgumentNullException(nameof(moduleCache));
    protected readonly ILProcessor Processor = processor ?? throw new ArgumentNullException(nameof(processor));
    protected readonly List<Instruction> Instructions = [];
    
    public InstructionSet Build()
    {
        return new InstructionSet()
        {
            Instructions = Instructions.ToArray()
        };
    }
}
