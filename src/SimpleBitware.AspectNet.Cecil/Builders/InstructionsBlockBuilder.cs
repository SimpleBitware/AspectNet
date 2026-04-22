using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MoreLinq.Extensions;
using SimpleBitware.AspectNet.Cecil.Extensions;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Builders;

/// <summary>
/// Builds IL instruction blocks with support for instance variables, control flow, and property operations.
/// </summary>
/// <remarks>
/// This builder extends <see cref="InstructionSetBlockBuilderBase{TBuilder}"/> and provides specialized
/// methods for common IL patterns such as property assignment, value extraction, and exception handling.
/// </remarks>
public class InstructionsBlockBuilder(MethodDefinition method, ILProcessor processor, ModuleCache moduleCache) : InstructionSetBlockBuilderBase<InstructionsBlockBuilder>(method, processor, moduleCache)
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
    public InstructionsBlockBuilder ForEachAsOnion<T>(IEnumerable<T> items, InstructionSet initialInstructionSet, Func<InstructionsBlockBuilder, T, InstructionSet, InstructionSet> function)
    {
        var currentInstructionSet = initialInstructionSet;
        items
            .ForEach(x =>
            {
                var blockBuilder = new InstructionsBlockBuilder(Method, Processor, ModuleCache);
                currentInstructionSet = function(blockBuilder, x, currentInstructionSet);
            });
        Instructions.AddRange(currentInstructionSet.Instructions);
        return this;
    }
    
    /// <summary>
    /// Adds an instance variable block with an instance of type T created by the provided function.
    /// </summary>
    /// <typeparam name="T">The type of instance to create.</typeparam>
    /// <param name="function">A function that receives an <see cref="InstanceVariableBuilder"/> initialized with a new instance of T.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public InstructionsBlockBuilder AddInstanceVariableBlock<T>(Func<InstanceVariableBuilder, InstructionSet> function)
    {
        var instructionSet = function(new InstanceVariableBuilder(Method, Processor, ModuleCache).Create<T>());
        Instructions.AddRange(instructionSet.Instructions);
        return this;
    }
    
    /// <summary>
    /// Adds an instance variable block built by the provided function.
    /// </summary>
    /// <param name="function">A function that receives an <see cref="InstanceVariableBuilder"/> and returns an instruction set.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public InstructionsBlockBuilder AddInstanceVariableBlock(Func<InstanceVariableBuilder, InstructionSet> function)
    {
        var instructionSet = function(new InstanceVariableBuilder(Method, Processor, ModuleCache));
        Instructions.AddRange(instructionSet.Instructions);
        return this;
    }
    
    /// <summary>
    /// Adds a generic variable to the method body and initializes it with default value.
    /// </summary>
    /// <param name="variableDefinition">The variable to add, or null to skip addition.</param>
    /// <param name="typeReference">The type reference for initialization.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// If <paramref name="variableDefinition"/> is null, this method has no effect.
    /// Uses the <c>initobj</c> IL instruction to initialize value types to their default values.
    /// </remarks>
    public InstructionsBlockBuilder AddGenericVariable(VariableDefinition? variableDefinition, TypeReference typeReference)
    {
        if (variableDefinition == null) return this;

        Method.Body.Variables.Add(variableDefinition);
        Instructions.Add(Processor.Create(OpCodes.Ldloca, variableDefinition));
        Instructions.Add(Processor.Create(OpCodes.Initobj, typeReference));
        return this;
    }

    /// <summary>
    /// Loads a value from a variable and calls a method reference on it.
    /// </summary>
    /// <param name="parameter">The variable to load from.</param>
    /// <param name="methodReference">The method to call on the loaded value, or null to skip the operation.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// If <paramref name="methodReference"/> is null, this method has no effect.
    /// </remarks>
    public InstructionsBlockBuilder GetValue(VariableDefinition parameter, MethodReference? methodReference)
    {
        if (methodReference is not null)
        {
            Instructions.AddRange([
                Processor.Create(OpCodes.Ldloc, parameter),
                Processor.Create(OpCodes.Callvirt, methodReference)
            ]);
        }

        return this;
    }

    /// <summary>
    /// Sets a property value or uses a default value if the source is null.
    /// </summary>
    /// <param name="variableDefinition">The target variable to store the result in, or null to skip operation.</param>
    /// <param name="instance">The instance variable from which to retrieve the property value.</param>
    /// <param name="getMethod">The getter method reference, or null to skip operation.</param>
    /// <param name="returnTypeReference">The type reference of the return value for unboxing.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This method generates IL that retrieves a value from an instance, checks if it is null,
    /// and either unboxes the value or creates a default value for the specified type.
    /// </remarks>
    public InstructionsBlockBuilder SetPropertyOrDefault(
        VariableDefinition? variableDefinition,
        VariableDefinition instance,
        MethodReference? getMethod,
        TypeReference returnTypeReference)
    {
        if (variableDefinition is not null && getMethod is not null)
        {
            // Load val.ReturnValue onto stack
            Instructions.AddRange([
                Processor.Create(OpCodes.Ldloc, instance),
                Processor.Create(OpCodes.Callvirt, getMethod),
                Processor.Create(OpCodes.Dup)
            ]);

            // Prepare our jump targets
            var unboxTarget = Processor.Create(OpCodes.Unbox_Any, returnTypeReference);
            var finalStore = Processor.Create(OpCodes.Stloc, variableDefinition);

            // Null Check
            Instructions.Add(Processor.Create(OpCodes.Brtrue_S, unboxTarget));

            // Pop the duped null
            Instructions.Add(Processor.Create(OpCodes.Pop));
            if (returnTypeReference.IsValueType || returnTypeReference.IsGenericParameter)
            {
                // We need default(T) on the stack.
                // We use the variable as a temporary buffer to create it.
                Instructions.AddRange([
                    Processor.Create(OpCodes.Ldloca, variableDefinition),
                    Processor.Create(OpCodes.Initobj, returnTypeReference),
                    Processor.Create(OpCodes.Ldloc, variableDefinition)
                ]);
            }
            else
            {
                Instructions.Add(Processor.Create(OpCodes.Ldnull));
            }

            // Jump to the ONLY store
            Instructions.Add(Processor.Create(OpCodes.Br_S, finalStore));

            // --- NOT NULL PATH ---
            // Stack now has the unboxed value
            Instructions.Add(unboxTarget);

            // StackValue = (Condition) ? PathA_Stack : PathB_Stack;
            // Followed by: num = StackValue;
            Instructions.Add(finalStore);
        }

        return this;
    }
    
    /// <summary>
    /// Executes a method with the specified return type.
    /// </summary>
    /// <param name="methodInfo">The method to execute, or null to skip operation.</param>
    /// <param name="returnType">The return type reference for generic method instantiation.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// If <paramref name="methodInfo"/> is null, this method has no effect.
    /// </remarks>
    public InstructionsBlockBuilder Execute(MethodInfo? methodInfo, TypeReference returnType)
    {
        if (methodInfo is null) return this;

        var methodReference = ModuleCache.ImportReference(methodInfo)?.MakeGeneric(returnType);
        if (methodReference is not null)
            Instructions.Add(Processor.Create(OpCodes.Call, methodReference));

        return this;
    }

    /// <summary>
    /// Executes a method by loading variables and calling the method reference.
    /// </summary>
    /// <param name="variable">The variable to load first.</param>
    /// <param name="parameter">The parameter variable to pass to the method.</param>
    /// <param name="methodReference">The method reference to call, or null to skip operation.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// If <paramref name="methodReference"/> is null, this method has no effect.
    /// </remarks>
    public InstructionsBlockBuilder Execute(
        VariableDefinition variable,
        VariableDefinition parameter,
        MethodReference? methodReference)
    {
        if (methodReference is not null)
        {
            Instructions.Add(Processor.Create(OpCodes.Ldloc, variable));
            GetValue(parameter, methodReference);
        }

        return this;
    }
    
    /// <summary>
    /// Generates IL to rethrow an exception when a condition is equal to a specific value.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This method generates IL that branches away if values are not equal, otherwise rethrows the current exception.
    /// </remarks>
    public InstructionsBlockBuilder RethrowWhenEqual()
    {
        var skipToInstruction = Processor.Create(OpCodes.Nop);
        Instructions.AddRange([
            Processor.Create(OpCodes.Bne_Un_S, skipToInstruction),
            Processor.Create(OpCodes.Rethrow),
            skipToInstruction
        ]);

        return this;
    }

    /// <summary>
    /// Generates IL to throw an exception if a condition is true.
    /// </summary>
    /// <param name="variable">The variable to use in the condition check.</param>
    /// <param name="methodReference">The method to call to obtain the exception value.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// If the condition is false, execution branches around the throw instruction.
    /// </remarks>
    public InstructionsBlockBuilder ThrowWhenDifferent(VariableDefinition variable, MethodReference? methodReference)
    {
        var skipToInstruction = Processor.Create(OpCodes.Nop);
        Instructions.Add(Processor.Create(OpCodes.Brfalse_S, skipToInstruction));
        GetValue(variable, methodReference);
        Instructions.AddRange([
            Processor.Create(OpCodes.Throw),
            skipToInstruction
        ]);

        return this;
    }

    /// <summary>
    /// Conditionally executes an action based on the provided condition delegate.
    /// </summary>
    /// <param name="condition">A delegate that returns a boolean condition to evaluate.</param>
    /// <param name="action">The action to execute if the condition returns true.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This method allows for conditional builder logic at configuration time,
    /// evaluating the condition immediately rather than generating IL conditional code.
    /// </remarks>
    public InstructionsBlockBuilder ExecuteIf(Func<bool> condition, Action<InstructionsBlockBuilder> action)
    {
        if (condition())
        {
            action(this);
        }

        return this;
    }
}
