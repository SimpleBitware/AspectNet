using Mono.Cecil;
using Mono.Cecil.Cil;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Builders;

/// <summary>
/// Abstract base class for building instruction blocks with support for variables, exception handlers, and nested blocks.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type derived from this base class.</typeparam>
/// <remarks>
/// Extends <see cref="InstructionSetBuilderBase{TBuilder}"/> with additional capabilities for managing
/// variables, exception handlers, and nested instruction blocks. This class is used as the foundation
/// for more specialized builders like <see cref="InstructionsBlockBuilder"/> and <see cref="MethodBodyBuilder"/>.
/// </remarks>
public abstract class InstructionSetBlockBuilderBase<TBuilder>(MethodDefinition method, ILProcessor processor, ModuleCache moduleCache) 
    : InstructionSetBuilderBase<TBuilder>(method, processor, moduleCache)
    where TBuilder : InstructionSetBlockBuilderBase<TBuilder>
{
    /// <summary>
    /// Adds a variable definition to the method body.
    /// </summary>
    /// <param name="variableDefinition">The variable to add, or null to skip addition.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// If <paramref name="variableDefinition"/> is null, this method has no effect.
    /// </remarks>
    public TBuilder AddVariable(VariableDefinition? variableDefinition)
    {
        if (variableDefinition is not null)
            Method.Body.Variables.Add(variableDefinition);
    
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Adds an exception handler to the method body.
    /// </summary>
    /// <param name="exceptionHandler">The exception handler to add.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public TBuilder AddExceptionHandler(ExceptionHandler exceptionHandler)
    {
        Method.Body.ExceptionHandlers.Add(exceptionHandler);
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Adds a collection of IL instructions to the instruction set.
    /// </summary>
    /// <param name="instructionItems">The instructions to add.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public TBuilder AddInstructions(IEnumerable<Instruction> instructionItems)
    {
        Instructions.AddRange(instructionItems);
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Adds instructions from an existing instruction set.
    /// </summary>
    /// <param name="instructionSet">The instruction set containing instructions to add.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public TBuilder AddInstructions(InstructionSet instructionSet)
    {
        Instructions.AddRange(instructionSet.Instructions);
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Creates and adds an instruction block built by the provided function.
    /// </summary>
    /// <param name="function">A delegate that receives an <see cref="InstructionsBlockBuilder"/> and returns an <see cref="InstructionSet"/>.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This method enables nested building patterns where instructions can be constructed
    /// in isolated blocks and then merged into the current builder.
    /// </remarks>
    public TBuilder AddInstructionsBlock(Func<InstructionsBlockBuilder, InstructionSet> function)
    {
        var blockBuilder = new InstructionsBlockBuilder(Method, Processor, ModuleCache);
        var instructionSet = function(blockBuilder);
        Instructions.AddRange(instructionSet.Instructions);
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Creates and adds a try-catch-finally block built by the provided function.
    /// </summary>
    /// <param name="function">A delegate that receives a <see cref="TryCatchBuilder"/> and returns an <see cref="InstructionSet"/>.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This method simplifies the creation of exception handling structures by delegating
    /// to a specialized try-catch builder.
    /// </remarks>
    public TBuilder AddTryCatchBlock(Func<TryCatchBuilder, InstructionSet> function)
    {
        var blockBuilder = new TryCatchBuilder(Method, Processor, ModuleCache);
        var instructionSet = function(blockBuilder);
        Instructions.AddRange(instructionSet.Instructions);
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Conditionally executes an action based on the provided condition.
    /// </summary>
    /// <param name="condition">The boolean condition to evaluate.</param>
    /// <param name="action">The action to execute if the condition is true.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This method allows for conditional builder logic at configuration time,
    /// not to be confused with runtime IL conditions.
    /// </remarks>
    public TBuilder ExecuteIf(bool condition, Action<TBuilder> action)
    {
        if (condition)
            action((TBuilder)this);
    
        return (TBuilder)this;
    }
}
