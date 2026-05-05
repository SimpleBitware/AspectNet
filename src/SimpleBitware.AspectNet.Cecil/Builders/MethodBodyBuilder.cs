using Mono.Cecil;
using Mono.Cecil.Cil;
using SimpleBitware.AspectNet.Cecil.Extensions;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Builders;

/// <summary>
/// Builds complete method bodies with IL instructions and exception handling.
/// </summary>
/// <remarks>
/// This class extends <see cref="InstructionSetBlockBuilderBase{TBuilder}"/> to provide
/// methods for constructing and modifying entire method bodies during IL weaving.
/// It manages instruction sequences and exception handlers for the target method.
/// </remarks>
public class MethodBodyBuilder(MethodDefinition method, ILProcessor processor, ModuleCache moduleCache) : InstructionSetBlockBuilderBase<MethodBodyBuilder>(method, processor, moduleCache)
{
    /// <summary>
    /// Clears all instructions and exception handlers from the method body.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This method is typically called at the start of method weaving to prepare
    /// the method body for new IL instructions.
    /// </remarks>
    public MethodBodyBuilder ClearMethodBody()
    {
        Method.Body.Instructions.Clear();
        Method.Body.ExceptionHandlers.Clear();
        return this;
    }

    /// <summary>
    /// Adds a return instruction, optionally returning a value from the specified variable.
    /// </summary>
    /// <param name="variableDefinition">The variable to return, or null to return void.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// If <paramref name="variableDefinition"/> is provided, its value is loaded onto the stack
    /// before the return instruction is emitted. If null, a simple void return is performed.
    /// This method generates the final return sequence for a method body.
    /// </remarks>
    public MethodBodyBuilder AddReturn(VariableDefinition? variableDefinition, TypeReference returnType, bool isAsync, ModuleDefinition module)
    {
        // If we have a result to return, load it onto the stack
        if (variableDefinition != null)
        {
            Instructions.Add(Processor.Create(OpCodes.Ldloc, variableDefinition));
        }

        // Simply emit the return instruction.
        // For both Async (Task/ValueTask) and Sync methods, the variableDefinition 
        // already holds the final 'wrapped' result or the direct return value.
        Instructions.Add(Processor.Create(OpCodes.Ret));

        return this;
    }

    private MethodReference ImportTaskFromResult(ModuleDefinition module, TypeReference innerT)
    {
        // Use ImportReference directly—it handles System.Type naturally
        var taskType = ModuleCache.ImportReference(typeof(Task)).Resolve();

        var method = taskType.Methods.First(m => m.Name == "FromResult" && m.HasGenericParameters);

        var genericMethod = new GenericInstanceMethod(module.ImportReference(method));
        genericMethod.GenericArguments.Add(module.ImportReference(innerT));

        return genericMethod;
    }
}
