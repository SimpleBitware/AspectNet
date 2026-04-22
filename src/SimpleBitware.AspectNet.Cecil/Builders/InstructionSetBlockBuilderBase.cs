using Mono.Cecil;
using Mono.Cecil.Cil;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Builders;

public abstract class InstructionSetBlockBuilderBase<TBuilder>(MethodDefinition method, ILProcessor processor, ModuleCache moduleCache) 
    : InstructionSetBuilderBase<TBuilder>(method, processor, moduleCache)
    where TBuilder : InstructionSetBlockBuilderBase<TBuilder>
{
    public TBuilder AddVariable(VariableDefinition? variableDefinition)
    {
        if (variableDefinition is not null)
            Method.Body.Variables.Add(variableDefinition);
    
        return (TBuilder)this;
    }
    
    public TBuilder AddExceptionHandler(ExceptionHandler exceptionHandler)
    {
        Method.Body.ExceptionHandlers.Add(exceptionHandler);
        return (TBuilder)this;
    }
    
    public TBuilder AddInstructions(IEnumerable<Instruction> instructionItems)
    {
        Instructions.AddRange(instructionItems);
        return (TBuilder)this;
    }
    
    public TBuilder AddInstructions(InstructionSet instructionSet)
    {
        Instructions.AddRange(instructionSet.Instructions);
        return (TBuilder)this;
    }
    
    public TBuilder AddInstructionsBlock(Func<InstructionsBlockBuilder, InstructionSet> function)
    {
        var blockBuilder = new InstructionsBlockBuilder(Method, Processor, ModuleCache);
        var instructionSet = function(blockBuilder);
        Instructions.AddRange(instructionSet.Instructions);
        return (TBuilder)this;
    }
    
    public TBuilder AddTryCatchBlock(Func<TryCatchBuilder, InstructionSet> function)
    {
        var blockBuilder = new TryCatchBuilder(Method, Processor, ModuleCache);
        var instructionSet = function(blockBuilder);
        Instructions.AddRange(instructionSet.Instructions);
        return (TBuilder)this;
    }
    
    public TBuilder ExecuteIf(bool condition, Action<TBuilder> action)
    {
        if (condition)
            action((TBuilder)this);
    
        return (TBuilder)this;
    }
}
