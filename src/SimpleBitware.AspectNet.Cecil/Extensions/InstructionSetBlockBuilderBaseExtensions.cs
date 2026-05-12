using Mono.Cecil;
using Mono.Cecil.Cil;
using MoreLinq;
using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Cecil.Builders;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

public static class InstructionSetBlockBuilderBaseExtensions
{
    public static InstructionsSetBlockBuilder AddTryBlockForAsyncMethods(
        this InstructionsSetBlockBuilder builder,
        ModuleCache moduleCache,
        VariableDefinition? returnValueVariableDefinition,
        VariableDefinition contextVariableDefinition,
        VariableDefinition aspectVariableDefinition,
        TypeReference returnTypeReference,
        bool isInnermost,
        InstructionSet currentInstructionSet)
    {
        if (returnValueVariableDefinition is null)
            return builder;

        // --- 1. Resolve and Import RunAsync Method ---
        var isGeneric = returnTypeReference.IsGenericInstance;
        var isValueTask = returnTypeReference.IsValueTaskType();

        var runnerTypeRef = moduleCache.ImportReference(typeof(AsyncAspectRunner));
        var runnerTypeDefinition = moduleCache.Resolve(runnerTypeRef);

        var runAsyncRunnerMethodDefinition = runnerTypeDefinition.Methods
            .First(m =>
                m.Name == nameof(AsyncAspectRunner.RunAsync) &&
                m.HasGenericParameters == isGeneric &&
                m.Parameters.Any(p => isValueTask ? p.ParameterType.IsValueTaskType() : p.ParameterType.IsTaskType())
            );

        MethodReference importedMethod;
        if (isGeneric && returnTypeReference is GenericInstanceType genericVt)
        {
            // Extract the T from Task<T> or ValueTask<T>
            var T = genericVt.GenericArguments[0];
            var methodRef = moduleCache.Module.ImportReference(runAsyncRunnerMethodDefinition);
            var closedMethod = new GenericInstanceMethod(methodRef);

            // Ensure T is imported into the current module to avoid verification errors
            closedMethod.GenericArguments.Add(moduleCache.ImportReference(T));
            importedMethod = closedMethod;
        }
        else
        {
            importedMethod = moduleCache.Module.ImportReference(runAsyncRunnerMethodDefinition);
        }

        var successLandingInstruction = builder.CreateEmptyInstruction();
        var oldReturnInstruction = currentInstructionSet.Instructions.LastOrDefault(x => x.OpCode.Code == Code.Ret);

        var sanitizedInnerInstructions = currentInstructionSet.Instructions
            .SkipLastWhile(x => x.OpCode.Code == Code.Ret)
            .ToList()
            .ApplyPeepholeOptimization();

        if (oldReturnInstruction != null)
            RedirectLogicalBranches(sanitizedInnerInstructions, oldReturnInstruction, successLandingInstruction);

        return builder
            .AddInstructions(sanitizedInnerInstructions)
            .AddInstructions([successLandingInstruction]) // The "Gate"
            .If(isInnermost,
                ifBlockBuilder => ifBlockBuilder
                    .SetVariable(variableBuilder => variableBuilder
                        // Innermost captures the Task from the state machine's .Task getter/stack
                        .AssignResultToVariable(returnValueVariableDefinition)
                        .Build()
                    )
                    .Build()
            )
            .ExecuteStaticMethod(importedMethod, returnValueVariableDefinition, aspectVariableDefinition, contextVariableDefinition)
            .SetVariable(variableBuilder => variableBuilder
                .AssignResultToVariable(returnValueVariableDefinition)
                .Build()
            );
    }
    
    public static InstructionsSetBlockBuilder AddFinallyBlockForAsyncMethods(
        this InstructionsSetBlockBuilder blockBuilder,
        VariableDefinition aspectVariableDefinition,
        AspectReferences aspectReferences,
        VariableDefinition contextVariableDefinition,
        VariableDefinition catchExecutedVariableDefinition)
    {
        var skipExit = blockBuilder.CreateEmptyInstruction();
        return blockBuilder
            .GoToWhenFalse(catchExecutedVariableDefinition, skipExit)
            .ExecuteInstanceMethod(aspectVariableDefinition, aspectReferences.OnExit, contextVariableDefinition)
            .AddInstructions([skipExit]);
    }

    public static InstructionsSetBlockBuilder AddTryBlockForSyncMethods(
        this InstructionsSetBlockBuilder blockBuilder,
        VariableDefinition? returnValueVariableDefinition,
        VariableDefinition contextVariableDefinition,
        VariableDefinition aspectVariableDefinition,
        AspectReferences aspectReferences,
        bool isInnermost,
        InstructionSet currentInstructionSet)
    {
        var successLandingInstruction = blockBuilder.CreateEmptyInstruction();
        var oldReturnInstruction = currentInstructionSet.Instructions.LastOrDefault(x => x.OpCode.Code == Code.Ret);
        var sanitizedInnerInstructions = currentInstructionSet.Instructions
            .SkipLastWhile(x => x.OpCode == OpCodes.Ret)
            .ToList();

        if (oldReturnInstruction != null)
            RedirectLogicalBranches(sanitizedInnerInstructions, oldReturnInstruction, successLandingInstruction);

        return blockBuilder
            .AddInstructions(sanitizedInnerInstructions)
            .AddInstructions([successLandingInstruction])
            .If(returnValueVariableDefinition != null,
                tryInstructionsBlockBuilder => tryInstructionsBlockBuilder
                    .If(isInnermost, innerInstructionsBlockBuilder =>
                        innerInstructionsBlockBuilder
                            .SetVariable(tryReturnInstanceBlockBuilder =>
                                tryReturnInstanceBlockBuilder
                                    .AssignResultToVariable(returnValueVariableDefinition)
                                    .Build()
                            )
                            .Build()
                    )
                    .SetVariable(assignReturnValueToContextBlockBuilder =>
                        assignReturnValueToContextBlockBuilder
                            .SetObjectProperty(
                                contextVariableDefinition,
                                typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.ReturnValue)),
                                returnValueVariableDefinition)
                            .Build()
                    )
                    .Build()
            )
            .ExecuteInstanceMethod(aspectVariableDefinition, aspectReferences.OnSuccess, contextVariableDefinition);
    }

    public static InstructionsSetBlockBuilder AddFinallyBlockForSyncMethods(
        this InstructionsSetBlockBuilder blockBuilder,
        VariableDefinition aspectVariableDefinition,
        AspectReferences aspectReferences,
        VariableDefinition contextVariableDefinition,
        AspectContextReferences contextReferences,
        TypeReference returnTypeReference,
        VariableDefinition? returnValueVariableDefinition)
    {
        return blockBuilder
            .ExecuteInstanceMethod(aspectVariableDefinition, aspectReferences.OnExit, contextVariableDefinition)
            .If(returnValueVariableDefinition != null, ifBuilder =>
                ifBuilder.SetPropertyOrDefault(returnValueVariableDefinition, contextVariableDefinition, contextReferences.ReturnValueGetMethod, returnTypeReference));
    }

    private static void RedirectLogicalBranches(IEnumerable<Instruction> instructions, Instruction oldTarget, Instruction newTarget)
    {
        foreach (var instruction in instructions)
        {
            // Handle standard branches (br, br_false, beq, etc.)
            if (instruction.Operand is Instruction target && target == oldTarget)
                instruction.Operand = newTarget;

            // Handle switch statements
            if (instruction.Operand is not Instruction[] targets) continue;
            for (var i = 0; i < targets.Length; i++)
            {
                if (targets[i] == oldTarget)
                    targets[i] = newTarget;
            }
        }
    }
}
