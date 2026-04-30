using Mono.Cecil;
using Mono.Cecil.Cil;
using MoreLinq;
using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Cecil.Builders;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

public static class InstructionSetBlockBuilderBaseExtensions
{
    public static List<Instruction> CreateSafeAsyncReturnInstructions(
        ILProcessor processor,
        ModuleDefinition module,
        VariableDefinition resultVar,
        TypeReference returnType)
    {
        var instructions = new List<Instruction>();

        // 1. Load the result onto the stack
        instructions.Add(processor.Create(OpCodes.Ldloc, resultVar));

        // 2. If it's a ValueTask, it's a struct (can't be null). Just return.
        if (returnType.FullName.Contains("ValueTask"))
        {
            instructions.Add(processor.Create(OpCodes.Ret));
            return instructions;
        }

        // 3. Setup null-check jump target
        var skipFallback = processor.Create(OpCodes.Nop);

        instructions.Add(processor.Create(OpCodes.Dup)); // Duplicate for null check
        instructions.Add(processor.Create(OpCodes.Brtrue_S, skipFallback)); // If not null, skip fallback
        instructions.Add(processor.Create(OpCodes.Pop)); // Pop the null

        // 4. Fallback Logic
        if (returnType.FullName == "System.Threading.Tasks.Task")
        {
            var taskType = module.ImportReference(typeof(Task)).Resolve();
            var getter = module.ImportReference(taskType.Properties.First(p => p.Name == "CompletedTask").GetMethod);
            instructions.Add(processor.Create(OpCodes.Call, getter));
        }
        else // Task<T>
        {
            var genericInstance = (GenericInstanceType)returnType;
            var T = genericInstance.GenericArguments[0];

            var taskType = module.ImportReference(typeof(Task)).Resolve();
            var fromResult = taskType.Methods.First(m => m.Name == "FromResult" && m.HasGenericParameters);
            var genericFromResult = new GenericInstanceMethod(module.ImportReference(fromResult));
            genericFromResult.GenericArguments.Add(module.ImportReference(T));

            // Load default(T) and call Task.FromResult
            instructions.AddRange(CreateDefaultValueInstructions(processor, T));
            instructions.Add(processor.Create(OpCodes.Call, genericFromResult));
        }

        // 5. Finalize
        instructions.Add(skipFallback);
        instructions.Add(processor.Create(OpCodes.Ret));

        return instructions;
    }

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

        // Crucial: Use moduleCache to ensure we aren't mixing assembly references (e.g., netstandard vs System.Runtime)
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

        // --- 2. Landing Pad & Branch Redirection ---
        // This creates a 'nop' that acts as the label for all early returns/branches
        var successLandingInstruction = builder.CreateEmptyInstruction();
        var oldReturnInstruction = currentInstructionSet.Instructions.LastOrDefault(x => x.OpCode.Code == Code.Ret);

        var sanitizedInnerInstructions = currentInstructionSet.Instructions
            .SkipLastWhile(x => x.OpCode.Code == Code.Ret) // Use Code.Ret for safer comparison
            .ToList()
            .ApplyPeepholeOptimization();

        // Redirect all internal logic that was heading for 'ret' to our wrapper entry point
        if (oldReturnInstruction != null)
        {
            RedirectLogicalBranches(sanitizedInnerInstructions, oldReturnInstruction, successLandingInstruction);
        }

        // --- 3. Build the Async Wrapper Chain ---
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
            // Wraps the task: task = AsyncAspectRunner.RunAsync(task, aspect, context)
            .ExecuteStaticMethod(importedMethod, returnValueVariableDefinition, aspectVariableDefinition, contextVariableDefinition)
            .SetVariable(variableBuilder => variableBuilder
                .AssignResultToVariable(returnValueVariableDefinition)
                .Build()
            );
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

    private static List<Instruction> CreateDefaultValueInstructions(ILProcessor processor, TypeReference type)
    {
        var instructions = new List<Instruction>();

        if (type.IsValueType)
        {
            switch (type.MetadataType)
            {
                case MetadataType.Boolean:
                case MetadataType.Int32:
                case MetadataType.SByte:
                case MetadataType.Int16:
                case MetadataType.Byte:
                case MetadataType.UInt16:
                case MetadataType.Char:
                    instructions.Add(processor.Create(OpCodes.Ldc_I4_0));
                    break;
                case MetadataType.Int64:
                case MetadataType.UInt64:
                    instructions.Add(processor.Create(OpCodes.Ldc_I8, 0L));
                    break;
                case MetadataType.Single:
                    instructions.Add(processor.Create(OpCodes.Ldc_R4, 0f));
                    break;
                case MetadataType.Double:
                    instructions.Add(processor.Create(OpCodes.Ldc_R8, 0d));
                    break;
                case MetadataType.Pointer:
                case MetadataType.FunctionPointer:
                    instructions.Add(processor.Create(OpCodes.Ldc_I4_0));
                    instructions.Add(processor.Create(OpCodes.Conv_I));
                    break;
                default:
                    var tempVar = new VariableDefinition(type);
                    processor.Body.Variables.Add(tempVar);
                    instructions.Add(processor.Create(OpCodes.Ldloca, tempVar));
                    instructions.Add(processor.Create(OpCodes.Initobj, type));
                    instructions.Add(processor.Create(OpCodes.Ldloc, tempVar));
                    break;
            }
        }
        else
        {
            instructions.Add(processor.Create(OpCodes.Ldnull));
        }

        return instructions;
    }

    private static void RedirectLogicalBranches(IEnumerable<Instruction> instructions, Instruction oldTarget, Instruction newTarget)
    {
        foreach (var instruction in instructions)
        {
            // Handle standard branches (br, brfalse, beq, etc.)
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
