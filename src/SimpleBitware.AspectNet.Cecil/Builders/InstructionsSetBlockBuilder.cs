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
public class InstructionsSetBlockBuilder(MethodDefinition method, ILProcessor processor, ModuleCache moduleCache) : InstructionSetBlockBuilderBase<InstructionsSetBlockBuilder>(method, processor, moduleCache)
{
    /// <summary>
    /// Loads a value from a variable and calls a method reference on it.
    /// </summary>
    /// <param name="parameter">The variable to load from.</param>
    /// <param name="methodReference">The method to call on the loaded value, or null to skip the operation.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// If <paramref name="methodReference"/> is null, this method has no effect.
    /// </remarks>
    public InstructionsSetBlockBuilder GetValue(VariableDefinition parameter, MethodReference? methodReference)
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
    public InstructionsSetBlockBuilder SetPropertyOrDefault(
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
    
    public InstructionsSetBlockBuilder SetPropertyIfNotNull(
        VariableDefinition? variableDefinition,
        VariableDefinition instance,
        MethodReference? getMethod,
        TypeReference returnTypeReference)
    {
        if (variableDefinition is not null && getMethod is not null)
        {
            // 1. Load context.ReturnValue (or target property) onto stack
            Instructions.Add(Processor.Create(OpCodes.Ldloc, instance));
            Instructions.Add(Processor.Create(OpCodes.Callvirt, getMethod));
            Instructions.Add(Processor.Create(OpCodes.Dup)); // Dup for the null check

            // 2. Prepare jump targets
            var unboxAndStoreTarget = Processor.Create(OpCodes.Unbox_Any, returnTypeReference);
            var endTarget = Processor.Create(OpCodes.Nop);

            // 3. If the value on stack is NOT null, jump to unbox and store it
            Instructions.Add(Processor.Create(OpCodes.Brtrue_S, unboxAndStoreTarget));

            // 4. --- NULL PATH ---
            // If it is null, we do NOTHING to the variable. 
            // We just pop the duplicated null and jump to the end.
            Instructions.Add(Processor.Create(OpCodes.Pop));
            Instructions.Add(Processor.Create(OpCodes.Br_S, endTarget));

            // 5. --- NOT NULL PATH ---
            Instructions.Add(unboxAndStoreTarget);
            Instructions.Add(Processor.Create(OpCodes.Stloc, variableDefinition));

            // 6. --- END ---
            Instructions.Add(endTarget);
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
    public InstructionsSetBlockBuilder Execute(MethodInfo? methodInfo, TypeReference returnType)
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
    public InstructionsSetBlockBuilder Execute(
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
    public InstructionsSetBlockBuilder RethrowWhenEqual()
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
    public InstructionsSetBlockBuilder ThrowWhenDifferent(VariableDefinition variable, MethodReference? methodReference)
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
}
