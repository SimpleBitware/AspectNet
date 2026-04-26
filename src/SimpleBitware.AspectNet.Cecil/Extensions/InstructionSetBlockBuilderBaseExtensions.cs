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
