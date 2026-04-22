using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using SimpleBitware.AspectNet.Abstractions;
using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Cecil.Builders;
using SimpleBitware.AspectNet.Cecil.Helpers;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

/// <summary>
/// Provides extension methods for weaving aspect-oriented programming logic into method definitions.
/// </summary>
/// <remarks>
/// This class contains the core weaving logic for AspectNet, including method body transformation,
/// try-catch-finally block generation, and aspect attribute processing.
/// </remarks>
public static class MethodDefinitionExtensions
{
    /// <summary>
    /// Applies a marker attribute to a weaved method to indicate it has been processed.
    /// </summary>
    /// <param name="method">The method definition to apply the marker to.</param>
    /// <param name="markerAttributeConstructor">The constructor of the marker attribute to apply.</param>
    /// <remarks>
    /// This method adds a custom attribute to the method to mark it as having been processed
    /// by the aspect weaver, preventing duplicate processing.
    /// </remarks>
    public static void ApplyMarkerAttribute(this MethodDefinition method, MethodReference markerAttributeConstructor)
    {
        method.CustomAttributes.Add(new CustomAttribute(markerAttributeConstructor));
    }

    /// <summary>
    /// Optimizes the macros in a method's body.
    /// </summary>
    /// <param name="method">The method definition to optimize.</param>
    /// <returns>The optimized method definition.</returns>
    /// <remarks>
    /// This method applies Mono.Cecil's macro optimizations to the method body,
    /// which can improve performance and reduce IL size.
    /// </remarks>
    public static MethodDefinition OptimizeMacros(this MethodDefinition method)
    {
        method.Body.OptimizeMacros();
        return method;
    }

    /// <summary>
    /// Weaves a method's body into try-catch-finally blocks for each AspectNet attribute.
    /// </summary>
    /// <param name="methodWithAspects">A key-value pair containing the method and its associated aspect attributes.</param>
    /// <returns>The weaved method definition.</returns>
    /// <remarks>
    /// This is the core weaving method that transforms a method to include aspect-oriented behavior.
    /// It creates nested try-catch-finally blocks for each aspect attribute, ordered by priority,
    /// and handles both synchronous and asynchronous methods.
    /// </remarks>
    public static MethodDefinition WeaveMethod(this KeyValuePair<MethodDefinition, CustomAttribute[]> methodWithAspects)
    {
        var method = methodWithAspects.Key;
        var aspectAttributes = methodWithAspects.Value;
        var methodStartInstructions = method.GetStartInstructions();
        var innerInstructions = method.GetInnerInstructions();
        var isAsyncMethod = method.ReturnType.IsTaskType();
        
        if(isAsyncMethod)
            return method;

        var moduleCache = method.Module.Cache();
        var processor = method.Body.GetILProcessor();
        var contextReferences = new AspectContextReferences(moduleCache);
        var aspectReferences = new AspectReferences(moduleCache);

        var contextVariableDefinition = new VariableDefinition(moduleCache.ImportReference(typeof(AspectNetAttributeContext)));
        var returnValueVariableDefinition = method.FindOrCreateReturnVariable();
        var returnTypeReference = moduleCache.ImportReference(method.ReturnType);
        var instructionSet = new InstructionSet()
        {
            Instructions = innerInstructions
        };

        var orderedAspects = aspectAttributes
            .Select((attribute, index) => (CustomAttribute: attribute, Index: index))
            .OrderBy(x => x.CustomAttribute.GetPriorityValue())
            .ThenBy(x => x.Index)
            .Select(x => x.CustomAttribute)
            .Reverse();

        new MethodBodyBuilder(method, processor, moduleCache)
            .ClearMethodBody()
            .AddVariable(contextVariableDefinition)
            .ExecuteIf(returnValueVariableDefinition != null && !method.Body.Variables.Contains(returnValueVariableDefinition),
                methodBodyBuilder => methodBodyBuilder.AddVariable(returnValueVariableDefinition))
            .AddInstructions(methodStartInstructions)
            .AddInstructionsBlock(instructionBlockBuilder => instructionBlockBuilder
                .AddGenericVariable(returnValueVariableDefinition, method.ReturnType)
                .AddInstanceVariableBlock<AspectNetAttributeContext>(instanceVariableBuilder => instanceVariableBuilder
                    .AssignResultToVariable(contextVariableDefinition)
                    .SetStringProperty(contextVariableDefinition, contextReferences.NameSetMethod, method.Name)
                    .SetObjectProperty(contextVariableDefinition, contextReferences.InstanceSetMethod, method.HasThis ? method.Body.ThisParameter : null)
                    .SetTypeProperty(contextVariableDefinition, typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.ClassType)), method.DeclaringType)
                    .SetDictionaryProperty<string, object>(contextVariableDefinition, typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.Parameters)), method.Parameters)
                    .Build()
                )
                .Build()
            )
            .AddInstructionsBlock(instructionBlockBuilder => instructionBlockBuilder
                .ForEachAsOnion(
                    orderedAspects,
                    instructionSet,
                    (builder, customAttribute, currentInstructionSet) =>
                    {
                        var isInnermost = (currentInstructionSet == instructionSet);
                        var aspectVariableDefinition = new VariableDefinition(moduleCache.ImportReference(customAttribute.AttributeType));
                        var exceptionVariableDefinition = new VariableDefinition(moduleCache.ImportReference(typeof(Exception)));
                        var builtInstructionSet = builder
                            .AddVariable(aspectVariableDefinition)
                            .AddVariable(exceptionVariableDefinition)
                            .Execute(typeof(AspectNetDependencyInjection).GetMethod(nameof(AspectNetDependencyInjection.GetRequiredService)), aspectVariableDefinition.VariableType)
                            .AddInstanceVariableBlock(instanceVariableBuilder => instanceVariableBuilder
                                .AssignResultToVariable(aspectVariableDefinition)
                                .SetIntProperty(
                                    aspectVariableDefinition,
                                    moduleCache.ImportReference(customAttribute.AttributeType, MemberNameHelper.PropertySetterName(nameof(IAspectNetAttribute.Priority)), 1),
                                    customAttribute.GetPriorityValue())
                                .Build()
                            )
                            .AddTryCatchBlock(tryCatchBuilder =>
                                tryCatchBuilder
                                    .StartTry()
                                    .AddInstructionsBlock(instructionsBlockBuilder => instructionsBlockBuilder
                                        .Execute(aspectVariableDefinition, contextVariableDefinition, aspectReferences.OnEntry)
                                        .AddInstructions(currentInstructionSet.Instructions.Where(i => i.OpCode != OpCodes.Ret))
                                        .ExecuteIf(returnValueVariableDefinition != null,
                                            tryInstructionsBlockBuilder => tryInstructionsBlockBuilder
                                                // Only the innermost layer consumes the stack value left by the original code.
                                                .ExecuteIf(isInnermost, innerInstructionsBlockBuilder => 
                                                    innerInstructionsBlockBuilder
                                                        .AddInstanceVariableBlock(tryReturnInstanceBlockBuilder =>
                                                            tryReturnInstanceBlockBuilder
                                                                .AssignResultToVariable(returnValueVariableDefinition)
                                                                .Build()
                                                        )
                                                        .Build()
                                                )
                                                .AddInstanceVariableBlock(assignReturnValueToContextBlockBuilder =>
                                                    assignReturnValueToContextBlockBuilder
                                                        .SetObjectProperty(
                                                            contextVariableDefinition,
                                                            typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.ReturnValue)),
                                                            returnValueVariableDefinition)
                                                        .Build()
                                                )
                                                .Build()
                                        )
                                        .Execute(aspectVariableDefinition, contextVariableDefinition, aspectReferences.OnSuccess)
                                        .Build()
                                    )
                                    .EndTry()
                                    .StartCatch()
                                    .AddInstructionsBlock(catchInstructionBlockBuilder => catchInstructionBlockBuilder
                                        .AddInstanceVariableBlock(assignExceptionToContextBlockBuilder =>
                                            assignExceptionToContextBlockBuilder
                                                .AssignResultToVariable(exceptionVariableDefinition)
                                                .SetObjectProperty(contextVariableDefinition,
                                                    typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.Exception)),
                                                    exceptionVariableDefinition)
                                                .Build()
                                        )
                                        .Execute(aspectVariableDefinition, contextVariableDefinition, aspectReferences.OnException)
                                        .Execute(exceptionVariableDefinition, contextVariableDefinition, contextReferences.ExceptionGetMethod)
                                        .RethrowWhenEqual()
                                        .GetValue(contextVariableDefinition, contextReferences.ExceptionGetMethod)
                                        .ThrowWhenDifferent(contextVariableDefinition, contextReferences.ExceptionGetMethod)
                                        .Build()
                                    )
                                    .EndCatch()
                                    .StartFinally()
                                    .AddInstructionsBlock(finallyInstructionBlockBuilder => finallyInstructionBlockBuilder
                                        .Execute(aspectVariableDefinition, contextVariableDefinition, aspectReferences.OnExit)
                                        // Restore return value if modified by aspect
                                        .ExecuteIf(() => returnValueVariableDefinition != null, b =>
                                            b.SetPropertyOrDefault(returnValueVariableDefinition, contextVariableDefinition, contextReferences.ReturnValueGetMethod, returnTypeReference))
                                        .Build()
                                    )
                                    .EndFinally()
                                    .Build())
                            .Build();
                        method.RemoveAttribute(customAttribute);
                        return builtInstructionSet;
                    })
                .Build()
            )
            .AddReturn(returnValueVariableDefinition)
            .Build()
            .ApplyTo(method);

        return method;
    }

    /// <summary>
    /// Removes a custom attribute from a method and its associated property if applicable.
    /// </summary>
    /// <param name="method">The method definition to remove the attribute from.</param>
    /// <param name="attribute">The custom attribute to remove.</param>
    /// <remarks>
    /// This method removes the attribute from the method's custom attributes collection.
    /// If the method is a property accessor, it also removes the attribute from the property itself.
    /// </remarks>
    private static void RemoveAttribute(this MethodDefinition method, CustomAttribute attribute)
    {
        method.CustomAttributes.Remove(attribute);

        var property = method.DeclaringType.Properties
            .FirstOrDefault(p => p.GetMethod == method || p.SetMethod == method);

        property?.CustomAttributes.Remove(attribute);
    }

    /// <summary>
    /// Creates a generic instance of a method reference with the specified type arguments.
    /// </summary>
    /// <param name="method">The method reference to make generic.</param>
    /// <param name="args">The type arguments to use for the generic method.</param>
    /// <returns>A generic instance method reference.</returns>
    /// <remarks>
    /// This method is used to create closed generic method references from open generic method definitions.
    /// </remarks>
    public static MethodReference MakeGeneric(this MethodReference method, params TypeReference[] args)
    {
        var genericType = new GenericInstanceMethod(method);
        foreach (var arg in args) genericType.GenericArguments.Add(arg);
        return genericType;
    }

    /// <summary>
    /// Gets the start instructions of a method, typically the base constructor call for constructors.
    /// </summary>
    /// <param name="method">The method definition to extract start instructions from.</param>
    /// <returns>An array of instructions that should be executed at the start of the method.</returns>
    /// <remarks>
    /// For constructors, this returns the instructions up to and including the base constructor call.
    /// For other methods, returns an empty array.
    /// </remarks>
    private static Instruction[] GetStartInstructions(this MethodDefinition method)
    {
        if (!method.IsConstructor)
            return [];

        var originalInstructions = method.Body.Instructions.ToList();
        var baseCall = originalInstructions.FirstOrDefault(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference { Name: Constants.InstanceConstructorMethodName });
        if (baseCall == null)
            return [];

        var index = originalInstructions.IndexOf(baseCall);
        return originalInstructions.Take(index + 1).ToArray();
    }

    /// <summary>
    /// Gets the inner instructions of a method, excluding the start instructions.
    /// </summary>
    /// <param name="method">The method definition to extract inner instructions from.</param>
    /// <returns>An array of instructions that form the main body of the method.</returns>
    /// <remarks>
    /// For constructors, this returns the instructions after the base constructor call.
    /// For other methods, returns all instructions.
    /// </remarks>
    private static Instruction[] GetInnerInstructions(this MethodDefinition method)
    {
        var originalInstructions = method.Body.Instructions.ToList();
        if (!method.IsConstructor)
            return originalInstructions.ToArray();

        var baseCall = originalInstructions.FirstOrDefault(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference { Name: Constants.InstanceConstructorMethodName });
        if (baseCall == null)
            return originalInstructions.ToArray();

        var index = originalInstructions.IndexOf(baseCall);
        return originalInstructions.Skip(index + 1).ToArray();
    }

    /// <summary>
    /// Finds or creates a return value variable for the method.
    /// </summary>
    /// <param name="method">The method definition to find or create a return variable for.</param>
    /// <returns>The return value variable, or null for void methods or constructors.</returns>
    /// <remarks>
    /// This method looks for an existing variable with the same type as the method's return type.
    /// If none exists and the method is not void, it creates a new variable.
    /// </remarks>
    private static VariableDefinition? FindOrCreateReturnVariable(this MethodDefinition method)
    {
        var isVoid = method.ReturnType.MetadataType == MetadataType.Void || method.IsConstructor;
        var returnVar = isVoid
            ? null
            : method.Body.Variables.FirstOrDefault(v => v.VariableType.FullName == method.ReturnType.FullName);

        if (!isVoid && returnVar == null)
        {
            returnVar = new VariableDefinition(method.ReturnType);
            method.Body.Variables.Add(returnVar);
        }

        return returnVar;
    }

    /// <summary>
    /// Wraps the asynchronous method in the attribute layer for AspectNet.
    /// </summary>
    /// <param name="method">The method definition to wrap.</param>
    /// <param name="customAttribute">The custom attribute associated with the aspect.</param>
    /// <param name="innerInstructions">The inner instructions of the method.</param>
    /// <param name="contextVariableDefinition">The context variable for aspect-oriented data.</param>
    /// <returns>The modified instructions for the method body.</returns>
    /// <remarks>
    /// This method generates the IL code to wrap an asynchronous method with the necessary
    /// aspect-oriented programming logic, including try-catch-finally blocks and aspect method calls.
    /// </remarks>
    private static Instruction[] WrapAsyncMethodInAttributeLayer(
        MethodDefinition method,
        CustomAttribute customAttribute,
        Instruction[] innerInstructions,
        VariableDefinition contextVariableDefinition)
    {
        var module = method.Module;
        var processor = method.Body.GetILProcessor();
        var returnType = method.ReturnType;
        var instructions = new List<Instruction>();

        // 1. Setup Variables
        var aspectType = module.ImportReference(customAttribute.AttributeType);
        var aspectVar = new VariableDefinition(aspectType);
        method.Body.Variables.Add(aspectVar);

        var successVar = new VariableDefinition(module.TypeSystem.Boolean);
        method.Body.Variables.Add(successVar);

        // This will hold the original task, and later the wrapped task
        var taskResultVar = new VariableDefinition(returnType);
        method.Body.Variables.Add(taskResultVar);

        var exceptionType = module.ImportReference(typeof(Exception));
        var exVar = new VariableDefinition(exceptionType); // 'ex'
        method.Body.Variables.Add(exVar);

        var ex2Var = new VariableDefinition(exceptionType); // 'ex2'
        method.Body.Variables.Add(ex2Var);

        // Resolve Context Exception Property Methods
        MethodReference? getExceptionMethod = null;
        MethodReference? setExceptionMethod = null;
        var contextTypeResolve = contextVariableDefinition.VariableType.Resolve();
        while (contextTypeResolve != null)
        {
            var getter = contextTypeResolve.Methods.FirstOrDefault(m => m.Name == "get_Exception");
            var setter = contextTypeResolve.Methods.FirstOrDefault(m => m.Name == "set_Exception");
            if (getter != null && setter != null)
            {
                getExceptionMethod = module.ImportReference(getter);
                setExceptionMethod = module.ImportReference(setter);
                break;
            }

            contextTypeResolve = contextTypeResolve.BaseType?.Resolve();
        }

        // --- PRE-TRY INITIALIZATION ---
        if (returnType.IsValueType || returnType.IsGenericParameter)
        {
            instructions.Add(processor.Create(OpCodes.Ldloca, taskResultVar));
            instructions.Add(processor.Create(OpCodes.Initobj, returnType));
        }
        else
        {
            instructions.Add(processor.Create(OpCodes.Ldnull));
            instructions.Add(processor.Create(OpCodes.Stloc, taskResultVar));
        }

        instructions.Add(processor.Create(OpCodes.Call, ImportGetRequiredService(module, customAttribute.AttributeType)));
        instructions.Add(processor.Create(OpCodes.Stloc, aspectVar));

// ----------------------------------
        // --- NEW: Set Priority Property ---
// Get the priority from the CustomAttribute metadata we're currently weaving
        int priorityValue = customAttribute.GetPriorityValue();

// Find the set_Priority method in the hierarchy
        var aspectTypeDefinition = customAttribute.AttributeType.Resolve();
        var setPriorityMethod = aspectTypeDefinition.Methods.FirstOrDefault(m => m.Name == "set_Priority");

// If not in the derived class, check the base class (AbstractAspectNetAttribute)
        if (setPriorityMethod == null && aspectTypeDefinition.BaseType != null)
        {
            var baseType = aspectTypeDefinition.BaseType.Resolve();
            setPriorityMethod = baseType.Methods.FirstOrDefault(m => m.Name == "set_Priority");
        }

        if (setPriorityMethod != null)
        {
            instructions.Add(processor.Create(OpCodes.Ldloc, aspectVar)); // Load aspect instance
            instructions.Add(processor.Create(OpCodes.Ldc_I4, priorityValue)); // Load the priority value (e.g., 3)
            instructions.Add(processor.Create(OpCodes.Callvirt, module.ImportReference(setPriorityMethod)));
        }
// ----------------------------------

        instructions.Add(processor.Create(OpCodes.Ldc_I4_0));
        instructions.Add(processor.Create(OpCodes.Stloc, successVar));

        // 2. Try Block Start
        var tryStart = processor.Create(OpCodes.Ldloc, aspectVar);
        instructions.Add(tryStart);

        instructions.Add(processor.Create(OpCodes.Ldloc, contextVariableDefinition));
        instructions.Add(processor.Create(OpCodes.Callvirt, ImportAspectMethod(module, "OnEntry")));

        var sanitizedInner = innerInstructions.ToList();
        if (sanitizedInner.Count > 0 && sanitizedInner[sanitizedInner.Count - 1].OpCode == OpCodes.Ret)
            sanitizedInner.RemoveAt(sanitizedInner.Count - 1);

        // Peephole Optimizer for .NET Standard 2.0
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

        instructions.AddRange(sanitizedInner);

        // --- WRAPPING INSIDE TRY ---
        // taskResultVar = originalTask;
        instructions.Add(processor.Create(OpCodes.Stloc, taskResultVar));

        // AsyncAspectRunner.WrapAsync(...)
        bool isValueTask = returnType.FullName.Contains("ValueTask");
        bool isGeneric = returnType.IsGenericInstance;

        // Load arguments for WrapAsync
        instructions.Add(processor.Create(OpCodes.Ldloc, taskResultVar));

        if (isValueTask)
        {
            // Convert ValueTask to Task if necessary
            var vtTempVar = new VariableDefinition(returnType);
            method.Body.Variables.Add(vtTempVar);
            instructions.Add(processor.Create(OpCodes.Stloc, vtTempVar));
            instructions.Add(processor.Create(OpCodes.Ldloca, vtTempVar));
            var vtTypeDef = returnType.Resolve();
            var openAsTask = vtTypeDef.Methods.First(m => m.Name == "AsTask");
            MethodReference asTaskMethod = isGeneric
                ? new MethodReference(openAsTask.Name, module.ImportReference(openAsTask.ReturnType, (GenericInstanceType)returnType), (GenericInstanceType)returnType) { HasThis = true }
                : module.ImportReference(openAsTask);
            instructions.Add(processor.Create(OpCodes.Call, asTaskMethod));
        }

        instructions.Add(processor.Create(OpCodes.Ldloc, contextVariableDefinition));
        instructions.Add(processor.Create(OpCodes.Ldloc, aspectVar));

        MethodReference wrapRunner;
        if (isGeneric)
        {
            var genericInstance = (GenericInstanceType)returnType;
            var openMethod = ImportRunnerMethod(module, "WrapAsync", true);
            var closedMethod = new GenericInstanceMethod(openMethod);
            closedMethod.GenericArguments.Add(module.ImportReference(genericInstance.GenericArguments[0], genericInstance));
            wrapRunner = module.ImportReference(closedMethod);
        }
        else wrapRunner = ImportRunnerMethod(module, "WrapAsync", false);

        instructions.Add(processor.Create(OpCodes.Call, wrapRunner));

        if (isValueTask)
        {
            // Convert Task back to ValueTask if necessary
            var vtTypeDef = returnType.Resolve();
            var openCtor = vtTypeDef.Methods.First(m => m.IsConstructor && m.Parameters.Count == 1);
            if (isGeneric)
            {
                var genericVT = (GenericInstanceType)returnType;
                var ctorRef = new MethodReference(".ctor", module.TypeSystem.Void, genericVT) { HasThis = true, CallingConvention = MethodCallingConvention.Default };
                ctorRef.Parameters.Add(new ParameterDefinition(module.ImportReference(openCtor.Parameters[0].ParameterType, genericVT)));
                instructions.Add(processor.Create(OpCodes.Newobj, ctorRef));
            }
            else instructions.Add(processor.Create(OpCodes.Newobj, module.ImportReference(openCtor)));
        }

        // Store the wrapped task and mark success
        instructions.Add(processor.Create(OpCodes.Stloc, taskResultVar));
        instructions.Add(processor.Create(OpCodes.Ldc_I4_1));
        instructions.Add(processor.Create(OpCodes.Stloc, successVar));

        var methodEnd = processor.Create(OpCodes.Ldloc, taskResultVar);
        instructions.Add(processor.Create(OpCodes.Leave, methodEnd));

        // 3. Catch Block
        var catchStart = processor.Create(OpCodes.Stloc, exVar);
        instructions.Add(catchStart);

        // Exception ex2 = (val.Exception = ex);
        instructions.Add(processor.Create(OpCodes.Ldloc, contextVariableDefinition));
        instructions.Add(processor.Create(OpCodes.Ldloc, exVar));
        instructions.Add(processor.Create(OpCodes.Dup)); // For assignment result
        instructions.Add(processor.Create(OpCodes.Stloc, ex2Var));
        instructions.Add(processor.Create(OpCodes.Callvirt, setExceptionMethod));

        instructions.Add(processor.Create(OpCodes.Ldloc, aspectVar));
        instructions.Add(processor.Create(OpCodes.Ldloc, contextVariableDefinition));
        instructions.Add(processor.Create(OpCodes.Callvirt, ImportAspectMethod(module, "OnException")));

        var checkNull = processor.Create(OpCodes.Ldloc, contextVariableDefinition);
        instructions.Add(processor.Create(OpCodes.Ldloc, ex2Var));
        instructions.Add(processor.Create(OpCodes.Ldloc, contextVariableDefinition));
        instructions.Add(processor.Create(OpCodes.Callvirt, getExceptionMethod));
        instructions.Add(processor.Create(OpCodes.Bne_Un_S, checkNull));
        instructions.Add(processor.Create(OpCodes.Rethrow));

        instructions.Add(checkNull);
        instructions.Add(processor.Create(OpCodes.Callvirt, getExceptionMethod));
        var leaveCatch = processor.Create(OpCodes.Leave, methodEnd);
        instructions.Add(processor.Create(OpCodes.Brfalse_S, leaveCatch));
        instructions.Add(processor.Create(OpCodes.Ldloc, contextVariableDefinition));
        instructions.Add(processor.Create(OpCodes.Callvirt, getExceptionMethod));
        instructions.Add(processor.Create(OpCodes.Throw));
        instructions.Add(leaveCatch);

        // 4. Finally Block
        var finallyStart = processor.Create(OpCodes.Ldloc, successVar);
        instructions.Add(finallyStart);
        var endFinally = processor.Create(OpCodes.Endfinally);
        instructions.Add(processor.Create(OpCodes.Brtrue_S, endFinally));

        instructions.Add(processor.Create(OpCodes.Ldloc, aspectVar));
        instructions.Add(processor.Create(OpCodes.Ldloc, contextVariableDefinition));
        instructions.Add(processor.Create(OpCodes.Callvirt, ImportAspectMethod(module, "OnExit")));
        instructions.Add(endFinally);

        // 5. Method Exit
        instructions.Add(methodEnd);
        instructions.Add(processor.Create(OpCodes.Ret));

        // Register Exception Handlers
        method.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            TryStart = tryStart, TryEnd = catchStart, HandlerStart = catchStart, HandlerEnd = finallyStart,
            CatchType = module.ImportReference(typeof(Exception))
        });

        method.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Finally)
        {
            TryStart = tryStart, TryEnd = finallyStart, HandlerStart = finallyStart, HandlerEnd = methodEnd
        });

        return instructions.ToArray();
    }

    /// <summary>
    /// Gets the variable index from a load/store local variable instruction.
    /// </summary>
    /// <param name="instruction">The instruction to analyze.</param>
    /// <returns>The variable index, or -1 if the instruction is not a local variable operation.</returns>
    /// <remarks>
    /// This method is used for peephole optimization to identify variable-related instructions.
    /// </remarks>
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

    /// <summary>
    /// Imports a method reference from the AsyncAspectRunner type.
    /// </summary>
    /// <param name="module">The module to import into.</param>
    /// <param name="name">The name of the method to import.</param>
    /// <param name="isGeneric">Whether the method is generic.</param>
    /// <returns>The imported method reference.</returns>
    /// <exception cref="Exception">Thrown when the method cannot be found.</exception>
    /// <remarks>
    /// This method is used to import methods from the AsyncAspectRunner utility class
    /// for handling asynchronous aspect operations.
    /// </remarks>
    private static MethodReference ImportRunnerMethod(ModuleDefinition module, string name, bool isGeneric)
    {
        // Replace 'AsyncAspectRunner' with the actual class name in your library
        var runnerTypeDef = module.ImportReference(typeof(AsyncAspectRunner)).Resolve();

        // We filter by name, parameter count (Task, Args, Aspect = 3), and generic status
        var method = runnerTypeDef.Methods.FirstOrDefault(m =>
            m.Name == name &&
            m.Parameters.Count == 3 &&
            m.HasGenericParameters == isGeneric);

        if (method == null)
            throw new Exception($"Could not find {name} in AsyncAspectRunner");

        return module.ImportReference(method);
    }

    /// <summary>
    /// Imports an aspect method reference from the AbstractAspectNetAttribute base type.
    /// </summary>
    /// <param name="module">The module to import into.</param>
    /// <param name="methodName">The name of the aspect method to import.</param>
    /// <returns>The imported method reference.</returns>
    /// <remarks>
    /// This method imports aspect lifecycle methods (OnEntry, OnExit, OnException, OnSuccess)
    /// from the base aspect attribute class.
    /// </remarks>
    private static MethodReference ImportAspectMethod(ModuleDefinition module, string methodName)
    {
        // Replace 'AbstractAspectNetAttribute' with your actual base class/interface
        var aspectBaseType = module.ImportReference(typeof(AbstractAspectNetAttribute)).Resolve();

        // Most aspect methods take exactly 1 parameter: the AspectEventArgs (context)
        return module.Cache().ImportReference(aspectBaseType, methodName, 1);
    }

    /// <summary>
    /// Imports the GetRequiredService generic method for dependency injection.
    /// </summary>
    /// <param name="module">The module to import into.</param>
    /// <param name="attributeType">The attribute type to use as the generic type argument.</param>
    /// <returns>The imported generic method reference.</returns>
    /// <remarks>
    /// This method creates a closed generic version of GetRequiredService&lt;T&gt;()
    /// where T is the specified attribute type, used for aspect instance resolution.
    /// </remarks>
    private static MethodReference ImportGetRequiredService(ModuleDefinition module, TypeReference attributeType)
    {
        var diType = module.ImportReference(typeof(AspectNetDependencyInjection)).Resolve();

        // Find the generic method: GetRequiredService<T>()
        var method = diType.Methods.FirstOrDefault(m =>
            m.Name == "GetRequiredService" &&
            m.HasGenericParameters &&
            m.Parameters.Count == 0);

        var methodRef = module.ImportReference(method);

        // Create the closed generic version: GetRequiredService<LogAttribute>()
        var genericMethod = new GenericInstanceMethod(methodRef);
        genericMethod.GenericArguments.Add(attributeType);

        return genericMethod;
    }
}
