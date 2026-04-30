using Mono.Cecil;
using Mono.Cecil.Cil;
using MoreLinq;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Builders;

/// <summary>
/// Abstract base class for building instruction blocks with support for variables, exception handlers, and nested blocks.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type derived from this base class.</typeparam>
/// <remarks>
/// Extends <see cref="InstructionSetBuilderBase{TBuilder}"/> with additional capabilities for managing
/// variables, exception handlers, and nested instruction blocks. This class is used as the foundation
/// for more specialized builders like <see cref="InstructionsSetBlockBuilder"/> and <see cref="MethodBodyBuilder"/>.
/// </remarks>
public abstract class InstructionSetBlockBuilderBase<TBuilder>(MethodDefinition method, ILProcessor processor, ModuleCache moduleCache)
    : InstructionSetBuilderBase<TBuilder>(method, processor, moduleCache)
    where TBuilder : InstructionSetBlockBuilderBase<TBuilder>
{
    /// <summary>
    /// Iterates over items using an onion pattern, where each iteration wraps the previous instructions.
    /// </summary>
    /// <typeparam name="T">The type of items to iterate over.</typeparam>
    /// <param name="items">The collection of items to iterate over.</param>
    /// <param name="initialInstructionSet">The initial instruction set to start with.</param>
    /// <param name="function">A function that produces an instruction set for each item, receiving the builder, item, and current instruction set.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// The onion pattern creates nested instruction blocks where each iteration's output becomes
    /// the input for the next iteration, creating a layered structure.
    /// </remarks>
    public TBuilder ForEachAsOnion<T>(IEnumerable<T> items, InstructionSet initialInstructionSet, Func<InstructionsSetBlockBuilder, T, InstructionSet, InstructionSet> function)
    {
        var currentInstructionSet = initialInstructionSet;
        items
            .ForEach(x =>
            {
                var blockBuilder = new InstructionsSetBlockBuilder(Method, Processor, ModuleCache);
                currentInstructionSet = function(blockBuilder, x, currentInstructionSet);
            });
        Instructions.AddRange(currentInstructionSet.Instructions);
        return (TBuilder)this;
    }

    /// <summary>
    /// Adds an instance variable block with an instance of type T created by the provided function.
    /// </summary>
    /// <typeparam name="T">The type of instance to create.</typeparam>
    /// <param name="function">A function that receives an <see cref="InstanceVariableBuilder"/> initialized with a new instance of T.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public TBuilder AddInstanceVariable<T>(Func<InstanceVariableBuilder, InstructionSet> function)
    {
        var instructionSet = function(new InstanceVariableBuilder(Method, Processor, ModuleCache).Create<T>());
        Instructions.AddRange(instructionSet.Instructions);
        return (TBuilder)this;
    }

    /// <summary>
    /// Adds an instance variable block built by the provided function.
    /// </summary>
    /// <param name="function">A function that receives an <see cref="InstanceVariableBuilder"/> and returns an instruction set.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public TBuilder SetVariable(Func<InstanceVariableBuilder, InstructionSet> function)
    {
        var instructionSet = function(new InstanceVariableBuilder(Method, Processor, ModuleCache));
        Instructions.AddRange(instructionSet.Instructions);
        return (TBuilder)this;
    }

    /// <summary>
    /// Adds a variable definition to the method body.
    /// </summary>
    /// <param name="variableDefinition">The variable to add, or null to skip addition.</param>
    /// <param name="function"></param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// If <paramref name="variableDefinition"/> is null, this method has no effect.
    /// </remarks>
    public TBuilder AddVariable(VariableDefinition? variableDefinition, Func<InstanceVariableBuilder, InstructionSet>? function = null)
    {
        if (variableDefinition is not null && !Method.Body.Variables.Contains(variableDefinition))
            Method.Body.Variables.Add(variableDefinition);
        
        if(function is not null)
        {
            var instructionSet = function(new InstanceVariableBuilder(Method, Processor, ModuleCache));
            Instructions.AddRange(instructionSet.Instructions);
        }

        return (TBuilder)this;
    }

    /// <summary>
    /// Assigns default value to the specified variable.
    /// </summary>
    /// <param name="variableDefinition">The variable to be set.</param>
    /// <param name="typeReference">The type reference for initialization.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// If <paramref name="variableDefinition"/> is null, this method has no effect.
    /// Uses the <c>initobj</c> IL instruction to initialize value types to their default values.
    /// </remarks>
    public TBuilder AssignDefaultValueToVariable(VariableDefinition? variableDefinition, TypeReference typeReference)
    {
        if (variableDefinition == null)
            return (TBuilder)this;

        Instructions.Add(Processor.Create(OpCodes.Ldloca, variableDefinition));
        Instructions.Add(Processor.Create(OpCodes.Initobj, typeReference));
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
    /// <param name="function">A delegate that receives an <see cref="InstructionsSetBlockBuilder"/> and returns an <see cref="InstructionSet"/>.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This method enables nested building patterns where instructions can be constructed
    /// in isolated blocks and then merged into the current builder.
    /// </remarks>
    public TBuilder AddInstructionsBlock(Func<InstructionsSetBlockBuilder, InstructionSet> function)
    {
        var blockBuilder = new InstructionsSetBlockBuilder(Method, Processor, ModuleCache);
        var instructionSet = function(blockBuilder);
        Instructions.AddRange(instructionSet.Instructions);
        return (TBuilder)this;
    }

    /// <summary>
    /// Creates and adds a try-catch-finally block built by the provided function.
    /// </summary>
    /// <param name="tryBlockBuilder">A delegate that receives a <see cref="InstructionsSetBlockBuilder"/> and returns an <see cref="InstructionSet"/>.</param>
    /// <param name="catchBlockBuilder">A delegate that receives a <see cref="InstructionsSetBlockBuilder"/> and returns an <see cref="InstructionSet"/>.</param>
    /// <param name="finallyBlockBuilder">A delegate that receives a <see cref="InstructionsSetBlockBuilder"/> and returns an <see cref="InstructionSet"/>.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This method simplifies the creation of exception handling structures by delegating
    /// to a specialized try-catch builder.
    /// </remarks>
    public TBuilder AddTryCatch(
        Func<InstructionsSetBlockBuilder, InstructionSet> tryBlockBuilder,
        Func<InstructionsSetBlockBuilder, InstructionSet> catchBlockBuilder,
        Func<InstructionsSetBlockBuilder, InstructionSet> finallyBlockBuilder)
    {
        var tryInstructionsSetBlockBuilder = new InstructionsSetBlockBuilder(Method, Processor, ModuleCache);
        var catchInstructionsSetBlockBuilder = new InstructionsSetBlockBuilder(Method, Processor, ModuleCache);
        var finallyInstructionsSetBlockBuilder = new InstructionsSetBlockBuilder(Method, Processor, ModuleCache);

        var blockBuilder = new TryCatchBuilder(Method, Processor, ModuleCache);
        blockBuilder
            .StartTry()
            .AddInstructions(tryBlockBuilder(tryInstructionsSetBlockBuilder))
            .EndTry()
            .StartCatch()
            .AddInstructions(catchBlockBuilder(catchInstructionsSetBlockBuilder))
            .EndCatch()
            .StartFinally()
            .AddInstructions(finallyBlockBuilder(finallyInstructionsSetBlockBuilder))
            .EndFinally();
        Instructions.AddRange(blockBuilder.Build().Instructions);
        return (TBuilder)this;
    }

    /// <summary>
    /// Conditionally executes an action based on the provided condition.
    /// </summary>
    /// <param name="condition">The boolean condition to evaluate.</param>
    /// <param name="ifAction">The action to execute if the condition is true.</param>
    /// <param name="elseAction">Optional action to execute if the condition is false.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This method allows for conditional builder logic at configuration time,
    /// not to be confused with runtime IL conditions.
    /// </remarks>
    public TBuilder If(bool condition, Action<TBuilder> ifAction, Action<TBuilder>? elseAction = null)
    {
        if (condition)
            ifAction((TBuilder)this);
        else
            elseAction?.Invoke((TBuilder)this);

        return (TBuilder)this;
    }

    public Instruction CreateEmptyInstruction()
    {
        return Processor.Create(OpCodes.Nop);
    }
}
