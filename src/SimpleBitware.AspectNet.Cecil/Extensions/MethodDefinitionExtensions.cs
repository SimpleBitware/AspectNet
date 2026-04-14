using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MoreLinq;
using SimpleBitware.AspectNet.Abstractions;
using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Cecil.Helpers;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

public static class MethodDefinitionExtensions
{
    /// <summary>
    /// Applies Marker Attribute to weaved method.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="markerAttributeConstructor"></param>
    public static void ApplyMarkerAttribute(this MethodDefinition method, MethodReference markerAttributeConstructor)
    {
        method.CustomAttributes.Add(new CustomAttribute(markerAttributeConstructor));
    }

    /// <summary>
    /// Optimizes method.
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public static MethodDefinition OptimizeMacros(this MethodDefinition method)
    {
        method.Body.OptimizeMacros();
        return method;
    }

    /// <summary>
    /// Weaves method's body into try-catch-finally block for each of the aspect net attributes.
    /// </summary>
    /// <param name="methodWithAspects"></param>
    /// <typeparam name="TContext"></typeparam>
    /// <returns>Weaved method definition.</returns>
    public static MethodDefinition WeaveMethod<TContext>(this KeyValuePair<MethodDefinition, CustomAttribute[]> methodWithAspects)
    {
        var method = methodWithAspects.Key;
        var aspectAttributes = methodWithAspects.Value;
        var processor = method.Body.GetILProcessor();
        var module = method.Module;
        var contextVariableDefinition = new VariableDefinition(module.ImportReference(typeof(TContext)));

        var methodStartInstructions = method.GetMethodStartInstructions();
        var methodInstructions = method.GetMethodInstructions();

        method.ClearMethodBody();

        aspectAttributes
            .Select((attribute, index) => new { attribute, index })
            .OrderBy(x => x.attribute.GetPriorityValue())
            .ThenBy(x => x.index)
            .Select(x => x.attribute)
            .Reverse()
            .ForEach(attribute =>
            {
                methodInstructions = method.ReturnType.IsTaskType()
                    ? WrapAsyncMethodInAttributeLayer(method, attribute, methodInstructions.ToArray(), contextVariableDefinition)
                    : WrapSyncMethodInAttributeLayer(method, attribute, methodInstructions.ToArray(), contextVariableDefinition);
                method.RemoveAttribute(attribute);
            });

        methodStartInstructions
            .Concat(processor.CreateAspectContext<TContext>(module, contextVariableDefinition, method))
            .Concat(methodInstructions)
            .Concat(processor.AddMethodReturn(method))
            .ForEach(processor.Append);
        
        if (!method.Body.Variables.Contains(contextVariableDefinition))
            method.Body.Variables.Add(contextVariableDefinition);
        
        return method;
    }

    private static void RemoveAttribute(this MethodDefinition method, CustomAttribute attribute)
    {
        method.CustomAttributes.Remove(attribute);

        var property = method.DeclaringType.Properties
            .FirstOrDefault(p => p.GetMethod == method || p.SetMethod == method);

        property?.CustomAttributes.Remove(attribute);
    }

    public static MethodReference MakeGeneric(this MethodReference method, params TypeReference[] args)
    {
        var genericType = new GenericInstanceMethod(method);
        foreach (var arg in args) genericType.GenericArguments.Add(arg);
        return genericType;
    }

    private static Instruction[] GetMethodInstructions(this MethodDefinition method)
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

    private static Instruction[] GetMethodStartInstructions(this MethodDefinition method)
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

    private static void ClearMethodBody(this MethodDefinition method)
    {
        method.Body.Instructions.Clear();
        method.Body.ExceptionHandlers.Clear();
    }

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

    private static Instruction[] WrapSyncMethodInAttributeLayer(
        MethodDefinition method,
        CustomAttribute customAttribute,
        Instruction[] innerInstructions,
        VariableDefinition contextVariableDefinition)
    {
        var processor = method.Body.GetILProcessor();
        var module = method.Module;
        var aspectReferences = new AspectReferences(module, customAttribute.AttributeType.Resolve());
        var contextExceptionGetMethod = module.ImportReference(typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.Exception))!.GetMethod);
        var contextExceptionSetMethod = module.ImportReference(typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.Exception))!.SetMethod);
        var contextReturnValueGetMethod = module.ImportReference(typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.ReturnValue))!.GetMethod);
        var contextReturnValueSetMethod = module.ImportReference(typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.ReturnValue))!.SetMethod);
        var returnTypeReference = module.ImportReference(method.ReturnType);

        // Layer Locals
        var aspectVariableDefinition = new VariableDefinition(module.ImportReference(customAttribute.AttributeType));
        var exceptionVariableDefinition = new VariableDefinition(module.ImportReference(typeof(Exception)));

        method.Body.Variables.Add(aspectVariableDefinition);
        method.Body.Variables.Add(exceptionVariableDefinition);

        // Find or Create Return Variable for this method
        var returnValueVariableDefinition = method.FindOrCreateReturnVariable();

        // Jump Targets
        var handlerTryStart = processor.Create(OpCodes.Nop);
        var handlerCatchStart = processor.Create(OpCodes.Nop);
        var handlerFinallyStart = processor.Create(OpCodes.Nop);
        var exitPoint = processor.Create(OpCodes.Nop); // The landing pad after everything

        // 1. Catch Handler: Protects the "Try" code
        var catchHandler = new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            TryStart = handlerTryStart,
            TryEnd = handlerCatchStart, // Ends where catch begins
            HandlerStart = handlerCatchStart,
            HandlerEnd = handlerFinallyStart, // Ends where finally begins
            CatchType = module.ImportReference(typeof(Exception))
        };

        // 2. Finally Handler: Protects BOTH the Try and the Catch
        var finallyHandler = new ExceptionHandler(ExceptionHandlerType.Finally)
        {
            TryStart = handlerTryStart,
            TryEnd = handlerFinallyStart, // Covers Try + Catch
            HandlerStart = handlerFinallyStart,
            HandlerEnd = exitPoint // Ends at the final exit nop
        };

        method.Body.ExceptionHandlers.Add(catchHandler);
        method.Body.ExceptionHandlers.Add(finallyHandler);

        return processor.CreateGetAspectInstanceBlock(module, aspectVariableDefinition)
            .Concat(processor.SetIntegerProperty(
                module,
                aspectVariableDefinition,
                customAttribute.AttributeType.Resolve().GetMethod(MemberNameHelper.PropertySetterName(nameof(IAspectNetAttribute.Priority))),
                customAttribute.GetPriorityValue()
            ))
            // --- START TRY ---
            .Append(handlerTryStart)
            .Concat(processor.CreateOnAspectMethodBlock(aspectVariableDefinition, contextVariableDefinition, aspectReferences.OnEntry))
            .Concat(innerInstructions.Where(x => x.OpCode != OpCodes.Ret))
            .Concat(processor.CreateOnAspectMethodBlock(aspectVariableDefinition, contextVariableDefinition, aspectReferences.OnSuccess))
            .Concat(processor.CreateMethodInnerInstructionsBlock(innerInstructions.Where(x => x.OpCode == OpCodes.Ret).ToArray(), returnValueVariableDefinition, exitPoint))
            .Append(processor.Create(OpCodes.Leave, exitPoint)) // This will trigger Finally then jump to exitPoint

            // --- START CATCH ---
            .Append(handlerCatchStart)
            .Concat(processor.CreateOnExceptionBlock(
                contextVariableDefinition,
                exceptionVariableDefinition,
                contextExceptionSetMethod,
                contextExceptionGetMethod,
                aspectVariableDefinition,
                aspectReferences.OnException))
            .Append(processor.Create(OpCodes.Leave, exitPoint))

            // --- START FINALLY ---
            .Append(handlerFinallyStart)
            .Concat(processor.CreateOnExitBlock(
                returnValueVariableDefinition,
                method.ReturnType.IsValueType,
                contextVariableDefinition,
                aspectVariableDefinition,
                contextReturnValueGetMethod,
                contextReturnValueSetMethod,
                returnTypeReference,
                aspectReferences.OnExit))
            .Append(processor.Create(OpCodes.Endfinally))

            // --- EXIT ---
            .Append(exitPoint)
            .ToArray();
    }

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
    //------

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

    private static MethodReference ImportAspectMethod(ModuleDefinition module, string methodName)
    {
        // Replace 'AbstractAspectNetAttribute' with your actual base class/interface
        var aspectBaseType = module.ImportReference(typeof(AbstractAspectNetAttribute)).Resolve();

        // Most aspect methods take exactly 1 parameter: the AspectEventArgs (context)
        return module.FindAndImport(aspectBaseType, methodName, 1);
    }

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
