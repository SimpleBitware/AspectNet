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
