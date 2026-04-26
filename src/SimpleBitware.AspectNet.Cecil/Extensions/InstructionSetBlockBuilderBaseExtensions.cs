using Mono.Cecil;
using Mono.Cecil.Cil;
using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Cecil.Builders;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

public static class InstructionSetBlockBuilderBaseExtensions
{
    private static int GetVariableIndex(Instruction instruction)
    {
        switch (instruction.OpCode.Code)
        {
            case Code.Ldloc_0:
            case Code.Stloc_0: return 0;
            case Code.Ldloc_1:
            case Code.Stloc_1: return 1;
            case Code.Ldloc_2:
            case Code.Stloc_2: return 2;
            case Code.Ldloc_3:
            case Code.Stloc_3: return 3;
            case Code.Ldloc_S:
            case Code.Stloc_S:
            case Code.Ldloc:
            case Code.Stloc:
                return (instruction.Operand as VariableDefinition)?.Index ?? -1;
            default: return -1;
        }
    }

    public static InstructionsSetBlockBuilder AddTryBlockForAsyncMethods(
        this InstructionsSetBlockBuilder builder,
        ILProcessor processor,
        ModuleDefinition module,
        ModuleCache moduleCache,
        VariableDefinition taskResultVariableDefinition,
        VariableDefinition contextVariableDefinition,
        VariableDefinition aspectVariableDefinition,
        TypeReference returnTypeReference,
        bool isGeneric,
        bool isInnermost,
        InstructionSet currentInstructionSet)
    {
        var elementType = returnTypeReference.GetElementType();
        bool isValueTask = elementType.Namespace == "System.Threading.Tasks" &&
                           elementType.Name.StartsWith("ValueTask");
        Console.WriteLine($"[*******] Return Type: {returnTypeReference.FullName}, Element Type: {elementType.FullName}, Is ValueTask: {isValueTask}");
        
        // --- PART A: SANITIZE INPUT ---
        var sanitizedInner = currentInstructionSet.Instructions.ToList();

        // Remove the trailing 'ret' if it exists, otherwise WrapAsync will never be reached
        if (sanitizedInner.Count > 0 && sanitizedInner[sanitizedInner.Count - 1].OpCode == OpCodes.Ret)
            sanitizedInner.RemoveAt(sanitizedInner.Count - 1);

        // Peephole Optimizer - ensure this isn't eating your actual logic
        if (sanitizedInner.Count >= 3)
        {
            var last = sanitizedInner[sanitizedInner.Count - 1];
            var prev1 = sanitizedInner[sanitizedInner.Count - 2];
            var prev2 = sanitizedInner[sanitizedInner.Count - 3];
            if (GetVariableIndex(last) != -1 && GetVariableIndex(last) == GetVariableIndex(prev2) &&
                (prev1.OpCode.Code == Code.Br || prev1.OpCode.Code == Code.Br_S))
            {
                sanitizedInner.RemoveAt(sanitizedInner.Count - 1);
                sanitizedInner.RemoveAt(sanitizedInner.Count - 1);
                sanitizedInner.RemoveAt(sanitizedInner.Count - 1);
            }
        }

        var instructions = new List<Instruction>();
        instructions.AddRange(sanitizedInner);

        // --- PART B: THE WRAPPING LOGIC ---

        // 1. If we are the innermost, the 'ValueTask' from the original body is on the stack.
        // We store it so we can pass it as the first argument to WrapAsync.
        if (isInnermost)
        {
            instructions.Add(processor.Create(OpCodes.Stloc, taskResultVariableDefinition));
        }

        // 2. Load arguments for WrapAsync(task, context, aspect)
        instructions.Add(processor.Create(OpCodes.Ldloc, taskResultVariableDefinition));
        instructions.Add(processor.Create(OpCodes.Ldloc, contextVariableDefinition));
        instructions.Add(processor.Create(OpCodes.Ldloc, aspectVariableDefinition));

        // 3. Resolve the WrapAsync overload
        var runnerTypeDef = moduleCache.ImportReference(typeof(AsyncAspectRunner)).Resolve();

        // Look for the specific overload (ValueTask vs Task)
        var wrapRunnerMethod = runnerTypeDef.Methods.First(m =>
            m.Name == "WrapAsync" &&
            m.HasGenericParameters == isGeneric &&
            m.Parameters[0].ParameterType.Name.StartsWith(isValueTask ? "ValueTask" : "Task")
        );

        MethodReference importedMethod;
        if (isGeneric && returnTypeReference is GenericInstanceType genericVt)
        {
            var T = genericVt.GenericArguments[0];
            var methodRef = module.ImportReference(wrapRunnerMethod);
            var closedMethod = new GenericInstanceMethod(methodRef);
            closedMethod.GenericArguments.Add(module.ImportReference(T));
            importedMethod = closedMethod;
        }
        else
        {
            importedMethod = module.ImportReference(wrapRunnerMethod);
        }

        // 4. Call WrapAsync and update the local variable with the new wrapped task
        instructions.Add(processor.Create(OpCodes.Call, importedMethod));
        instructions.Add(processor.Create(OpCodes.Stloc, taskResultVariableDefinition));

        return builder.AddInstructions(instructions);
    }

    public static InstructionsSetBlockBuilder AddFinallyBlockForAsyncMethods(
        this InstructionsSetBlockBuilder blockBuilder,
        VariableDefinition contextVariableDefinition,
        VariableDefinition aspectVariableDefinition,
        AspectReferences aspectReferences,
        AspectContextReferences contextReferences,
        ILProcessor processor)
    {
        // The instruction we jump to if context.Exception == null
        var skipOnExitInstruction = processor.Create(OpCodes.Nop);

        return blockBuilder.AddInstructions([
            // 1. Push context.Exception onto the stack
            processor.Create(OpCodes.Ldloc, contextVariableDefinition),
            processor.Create(OpCodes.Callvirt, contextReferences.ExceptionGetMethod),

            // 2. If null (false), jump over the OnExit call
            processor.Create(OpCodes.Brfalse_S, skipOnExitInstruction),

            // 3. --- context.Exception is NOT null ---
            // Push aspect instance
            processor.Create(OpCodes.Ldloc, aspectVariableDefinition),
            // Push context instance
            processor.Create(OpCodes.Ldloc, contextVariableDefinition),
            // Execute aspect.OnExit(context)
            processor.Create(OpCodes.Callvirt, aspectReferences.OnExit),

            // 4. --- Jump Target ---
            skipOnExitInstruction
        ]);
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
        return blockBuilder
            .AddInstructions(currentInstructionSet.Instructions.Where(i => i.OpCode != OpCodes.Ret))
            .If(returnValueVariableDefinition != null,
                tryInstructionsBlockBuilder => tryInstructionsBlockBuilder
                    // Only the innermost layer consumes the stack value left by the original code.
                    .If(isInnermost, innerInstructionsBlockBuilder =>
                        innerInstructionsBlockBuilder
                            .AddInstanceVariable(tryReturnInstanceBlockBuilder =>
                                tryReturnInstanceBlockBuilder
                                    .AssignResultToVariable(returnValueVariableDefinition)
                                    .Build()
                            )
                            .Build()
                    )
                    .AddInstanceVariable(assignReturnValueToContextBlockBuilder =>
                        assignReturnValueToContextBlockBuilder
                            .SetObjectProperty(
                                contextVariableDefinition,
                                typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.ReturnValue)),
                                returnValueVariableDefinition)
                            .Build()
                    )
                    .Build()
            )
            .Execute(aspectVariableDefinition, contextVariableDefinition, aspectReferences.OnSuccess);
    }

    public static InstructionsSetBlockBuilder AddFinallyBlockForSyncMethods(
        this InstructionsSetBlockBuilder blockBuilder,
        VariableDefinition? returnValueVariableDefinition,
        VariableDefinition contextVariableDefinition,
        VariableDefinition aspectVariableDefinition,
        AspectReferences aspectReferences,
        AspectContextReferences contextReferences,
        TypeReference returnTypeReference)
    {
        return blockBuilder.Execute(aspectVariableDefinition, contextVariableDefinition, aspectReferences.OnExit)
            // Restore return value if modified by aspect
            .If(returnValueVariableDefinition != null, b =>
                b.SetPropertyOrDefault(returnValueVariableDefinition, contextVariableDefinition, contextReferences.ReturnValueGetMethod, returnTypeReference));
    }
}
