using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
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
public class InstructionsSetBlockBuilder(MethodDefinition method, ILProcessor processor, ModuleCache moduleCache)
    : InstructionSetBlockBuilderBase<InstructionsSetBlockBuilder>(method, processor, moduleCache)
{
    /// <summary>
    /// Executes an instance method with specified parameters.
    /// </summary>
    /// <param name="instanceVariableDefinition">The object which has the method to be executed.</param>
    /// <param name="methodReference">The method to call on the loaded value, or null to skip the operation.</param>
    /// <param name="parameters">The parameters passed to the method to be executed</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// If <paramref name="methodReference"/> is null, this method has no effect.
    /// </remarks>
    public InstructionsSetBlockBuilder ExecuteInstanceMethod(
        VariableDefinition? instanceVariableDefinition,
        MethodReference? methodReference,
        params VariableDefinition[] parameters)
    {
        if (instanceVariableDefinition is not null && methodReference is not null)
        {
            var parameterInstructions = parameters
                .Select(p => Processor.Create(OpCodes.Ldloc, p))
                .ToArray();
            
            Instructions.Add(Processor.Create(OpCodes.Ldloc, instanceVariableDefinition));
            Instructions.AddRange(parameterInstructions);
            Instructions.Add(Processor.Create(OpCodes.Callvirt, methodReference));
        }
        
        return this;
    }
    
    /// <summary>
    /// Executes a static method with specified parameters.
    /// </summary>
    /// <param name="methodReference">The method to call on the loaded value, or null to skip the operation.</param>
    /// <param name="parameters">The parameters passed to the method to be executed</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// If <paramref name="methodReference"/> is null, this method has no effect.
    /// </remarks>
    public InstructionsSetBlockBuilder ExecuteStaticMethod(
        MethodReference? methodReference,
        params VariableDefinition[] parameters)
    {
        if (methodReference is not null)
        {
            var parameterInstructions = parameters
                .Select(p => Processor.Create(OpCodes.Ldloc, p))
                .ToArray();
            
            Instructions.AddRange(parameterInstructions);
            Instructions.Add(Processor.Create(OpCodes.Call, methodReference));
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

    public InstructionsSetBlockBuilder Execute(MethodInfo? methodInfo, TypeReference returnType)
    {
        if (methodInfo is null) return this;

        var methodReference = ModuleCache.ImportReference(methodInfo)?.MakeGeneric(returnType);
        if (methodReference is not null)
            ExecuteStaticMethod(methodReference);


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
        AddInstructions([
                Processor.Create(OpCodes.Brfalse_S, skipToInstruction)
            ])
            .ExecuteInstanceMethod(variable, methodReference)
            .AddInstructions([
                Processor.Create(OpCodes.Throw),
                skipToInstruction
            ]);

        return this;
    }

    public InstructionsSetBlockBuilder GoToWhenFalse(VariableDefinition condition, Instruction targetInstruction)
    {
        this.AddInstructions([
            Processor.Create(OpCodes.Ldloc, condition),
            Processor.Create(OpCodes.Brfalse_S, targetInstruction)
        ]);

        return this;
    }
}
