using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MoreLinq.Extensions;
using SimpleBitware.AspectNet.Cecil.Extensions;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Builders;

public class InstructionsBlockBuilder(MethodDefinition method, ILProcessor processor, ModuleCache moduleCache) : InstructionSetBlockBuilderBase<InstructionsBlockBuilder>(method, processor, moduleCache)
{
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
    
    public InstructionsBlockBuilder AddInstanceVariableBlock<T>(Func<InstanceVariableBuilder, InstructionSet> function)
    {
        var instructionSet = function(new InstanceVariableBuilder(Method, Processor, ModuleCache).Create<T>());
        Instructions.AddRange(instructionSet.Instructions);
        return this;
    }
    
    public InstructionsBlockBuilder AddInstanceVariableBlock(Func<InstanceVariableBuilder, InstructionSet> function)
    {
        var instructionSet = function(new InstanceVariableBuilder(Method, Processor, ModuleCache));
        Instructions.AddRange(instructionSet.Instructions);
        return this;
    }
    
    public InstructionsBlockBuilder AddGenericVariable(VariableDefinition? variableDefinition, TypeReference typeReference)
    {
        if (variableDefinition == null) return this;

        Method.Body.Variables.Add(variableDefinition);
        Instructions.Add(Processor.Create(OpCodes.Ldloca, variableDefinition));
        Instructions.Add(Processor.Create(OpCodes.Initobj, typeReference));
        return this;
    }

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
    
    public InstructionsBlockBuilder Execute(MethodInfo? methodInfo, TypeReference returnType)
    {
        if (methodInfo is null) return this;

        var methodReference = ModuleCache.ImportReference(methodInfo)?.MakeGeneric(returnType);
        if (methodReference is not null)
            Instructions.Add(Processor.Create(OpCodes.Call, methodReference));

        return this;
    }

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

    public InstructionsBlockBuilder ExecuteIf(Func<bool> condition, Action<InstructionsBlockBuilder> action)
    {
        if (condition())
        {
            action(this);
        }

        return this;
    }
}
