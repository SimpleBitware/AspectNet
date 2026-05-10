using Mono.Cecil;
using Mono.Cecil.Cil;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Builders;

/// <summary>
/// Abstract base class for building IL instruction sets with fluent API.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type derived from this base class, enabling fluent method chaining.</typeparam>
/// <remarks>
/// This class provides the foundation for all instruction builders in the AspectNet weaving framework.
/// It manages IL instructions, method definitions, IL processors, and module caching.
/// Derived classes extend this functionality with domain-specific instruction generation methods.
/// </remarks>
public abstract class InstructionSetBuilderBase<TBuilder>(MethodDefinition method, ILProcessor processor, ModuleCache moduleCache)
    where TBuilder : InstructionSetBuilderBase<TBuilder>
{
    /// <summary>
    /// Gets the method definition being modified.
    /// </summary>
    protected readonly MethodDefinition Method = method ?? throw new ArgumentNullException(nameof(method));
    
    /// <summary>
    /// Gets the module cache for importing types and methods.
    /// </summary>
    protected readonly ModuleCache ModuleCache = moduleCache ?? throw new ArgumentNullException(nameof(moduleCache));
    
    /// <summary>
    /// Gets the IL processor for creating IL instructions.
    /// </summary>
    protected readonly ILProcessor Processor = processor ?? throw new ArgumentNullException(nameof(processor));
    
    /// <summary>
    /// Gets the collection of IL instructions being built.
    /// </summary>
    protected readonly List<Instruction> Instructions = [];
    
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
    
    /// <summary>
    /// Builds and returns the final instruction set.
    /// </summary>
    /// <returns>An <see cref="InstructionSet"/> containing all accumulated IL instructions.</returns>
    public InstructionSet Build()
    {
        return new InstructionSet()
        {
            Instructions = Instructions.ToArray()
        };
    }
}
