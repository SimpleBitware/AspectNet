using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MoreLinq;
using SimpleBitware.AspectNet.Abstractions;
using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Abstractions.Context;
using SimpleBitware.AspectNet.Runtime.Cecil;

namespace SimpleBitware.AspectNet.Extensions.Cecil;

public static class MethodDefinitionExtensions
{
    /// <summary>
    /// Gets Method/Constructor-level aspect derived attributes
    /// </summary>
    /// <param name="method"></param>
    /// <param name="baseAspectNetAttribute"></param>
    /// <returns></returns>
    public static CustomAttribute[] GetMethodAspectNetDerivedAttributes(
        this MethodDefinition method,
        TypeDefinition baseAspectNetAttribute)
    {
        return method.CustomAttributes
            .Where(customAttribute => customAttribute.AttributeType.Resolve().InheritsFrom(baseAspectNetAttribute))
            .ToArray();
    }

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

        processor.AppendInstructions(methodStartInstructions)
            .CreateAspectContext<TContext>(module, contextVariableDefinition, method);

        aspectAttributes
            .Select((attribute, index) => new { attribute, index })
            .OrderByDescending(x => x.attribute.GetPriorityValue())
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

        processor.AppendInstructions(methodInstructions)
            .AddMethodReturn(method);

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

        var baseCall = originalInstructions.FirstOrDefault(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference { Name: ".ctor" });
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
        var baseCall = originalInstructions.FirstOrDefault(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference { Name: ".ctor" });
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
        var returnVar = isVoid ? null : method.Body.Variables.FirstOrDefault(v => v.VariableType.FullName == method.ReturnType.FullName);
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

        // 1. Setup Aspect Variable
        // This stores the instance of your attribute (e.g., LogAsyncAttribute)
        var aspectVariableDefinition = new VariableDefinition(module.ImportReference(customAttribute.AttributeType));
        method.Body.Variables.Add(aspectVariableDefinition);

        // 2. Aspect Instantiation & OnEntry
        // Get the aspect from DI: AspectNetDependencyInjection.GetRequiredService<T>()
        instructions.Add(processor.Create(OpCodes.Call, ImportGetRequiredService(module, customAttribute.AttributeType)));
        instructions.Add(processor.Create(OpCodes.Stloc, aspectVariableDefinition));

        // Call aspect.OnEntry(context)
        instructions.Add(processor.Create(OpCodes.Ldloc, aspectVariableDefinition));
        instructions.Add(processor.Create(OpCodes.Ldloc, contextVariableDefinition));
        instructions.Add(processor.Create(OpCodes.Callvirt, ImportAspectMethod(module, "OnEntry")));

        // 3. Process Inner Instructions (The original method work)
        // IMPORTANT: We remove the original 'ret' so the Task/ValueTask remains on the stack.
        var sanitizedInner = innerInstructions.ToList();
        if (sanitizedInner.LastOrDefault()?.OpCode == OpCodes.Ret)
        {
            sanitizedInner.RemoveAt(sanitizedInner.Count - 1);
        }

        instructions.AddRange(sanitizedInner);

        // 4. Wrap the Resulting Task
        bool isValueTask = returnType.FullName.Contains("ValueTask");
        bool isGeneric = returnType.IsGenericInstance;

        // If the method returns ValueTask, convert it to Task for the runner
        if (isValueTask)
        {
            var asTask = returnType.Resolve().Methods.First(m => m.Name == "AsTask");
            instructions.Add(processor.Create(OpCodes.Call, module.ImportReference(asTask)));
        }

        // Resolve the appropriate WrapAsync runner method
        MethodReference wrapMethod;
        if (isGeneric)
        {
            var genericType = (GenericInstanceType)returnType;
            var typeT = genericType.GenericArguments[0];
            var openMethod = ImportRunnerMethod(module, "WrapAsync", true);
            var closedMethod = new GenericInstanceMethod(openMethod);
            closedMethod.GenericArguments.Add(typeT);
            wrapMethod = closedMethod;
        }
        else
        {
            wrapMethod = ImportRunnerMethod(module, "WrapAsync", false);
        }

        // Stack is currently: [Task]
        // Load arguments for: WrapAsync(task, context, aspect)
        instructions.Add(processor.Create(OpCodes.Ldloc, contextVariableDefinition));
        instructions.Add(processor.Create(OpCodes.Ldloc, aspectVariableDefinition));
        instructions.Add(processor.Create(OpCodes.Call, wrapMethod));

        // 5. Convert back to ValueTask if necessary
        if (isValueTask)
        {
            var vtType = returnType.Resolve();
            var ctor = vtType.Methods.First(m => m.IsConstructor && m.Parameters.Count == 1);

            if (isGeneric)
            {
                var genericVT = (GenericInstanceType)returnType;
                var ctorRef = new MethodReference(".ctor", module.TypeSystem.Void, genericVT) { HasThis = true };
                ctorRef.Parameters.Add(new ParameterDefinition(module.ImportReference(wrapMethod.ReturnType)));
                instructions.Add(processor.Create(OpCodes.Newobj, module.ImportReference(ctorRef)));
            }
            else
            {
                instructions.Add(processor.Create(OpCodes.Newobj, module.ImportReference(ctor)));
            }
        }

        // 6. Final Return
        instructions.Add(processor.Create(OpCodes.Ret));

        return instructions.ToArray();
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
