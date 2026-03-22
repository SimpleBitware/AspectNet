using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MoreLinq;
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
    /// <typeparam name="TEntryContext"></typeparam>
    /// <returns>Weaved method definition.</returns>
    public static MethodDefinition WeaveMethod<TEntryContext, TExitContext>(this KeyValuePair<MethodDefinition, CustomAttribute[]> methodWithAspects)
    {
        var method = methodWithAspects.Key;
        var aspectAttributes = methodWithAspects.Value;
        var processor = method.Body.GetILProcessor();
        var module = method.Module;
        var entryContextVar = new VariableDefinition(module.ImportReference(typeof(TEntryContext)));
        var exitContextVar = new VariableDefinition(module.ImportReference(typeof(TExitContext)));

        var methodStartInstructions = method.GetMethodStartInstructions();
        var methodInstructions = method.GetMethodInstructions();

        method.ClearMethodBody();

        processor.AppendInstructions(methodStartInstructions)
            .CreateEntryContext<TEntryContext>(module, entryContextVar, method)
            .CreateExitContext<TExitContext>(module, exitContextVar, entryContextVar, method);

        aspectAttributes
            .OrderBy(customAttribute => customAttribute.GetPriorityValue())
            .Reverse()
            .ForEach(attribute =>
            {
                methodInstructions = WrapInAttributeLayer(method, attribute, methodInstructions.ToArray(), entryContextVar, exitContextVar);
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

    private static MethodReference MakeGeneric(this MethodReference method, params TypeReference[] args)
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

    private static Instruction[] WrapInAttributeLayer(
        MethodDefinition method,
        CustomAttribute customAttribute,
        Instruction[] innerInstructions,
        VariableDefinition entryContext,
        VariableDefinition exitContext)
    {
        var il = method.Body.GetILProcessor();
        var module = method.Module;
        var refs = new AspectReferences(module, customAttribute.AttributeType.Resolve());
        var isVoid = method.ReturnType.MetadataType == MetadataType.Void || method.IsConstructor;

        // Layer Locals
        var aspectVar = new VariableDefinition(module.ImportReference(customAttribute.AttributeType));
        var exceptionVar = new VariableDefinition(module.ImportReference(typeof(Exception)));
        var exCtxLocal = new VariableDefinition(module.ImportReference(typeof(AspectNetExceptionContext)));
        method.Body.Variables.Add(aspectVar);
        method.Body.Variables.Add(exceptionVar);
        method.Body.Variables.Add(exCtxLocal);

        // Find or Create Return Variable for this method
        VariableDefinition? returnVar = isVoid ? null : method.Body.Variables.FirstOrDefault(v => v.VariableType.FullName == method.ReturnType.FullName);
        if (!isVoid && returnVar == null)
        {
            returnVar = new VariableDefinition(method.ReturnType);
            method.Body.Variables.Add(returnVar);
        }

        // Jump Targets
        var handlerCatchStart = il.Create(OpCodes.Nop);
        var handlerFinallyStart = il.Create(OpCodes.Nop);
        var exitPoint = il.Create(OpCodes.Nop);

        List<Instruction> newLayer = new();

        // 1. Get Aspect Instance
        var getService = module.ImportReference(typeof(AspectNetDependencyInjection).GetMethod(nameof(AspectNetDependencyInjection.GetRequiredService)))
            .MakeGeneric(aspectVar.VariableType);
        newLayer.Add(il.Create(OpCodes.Call, getService));
        newLayer.Add(il.Create(OpCodes.Stloc, aspectVar));

        // 2. Start Try Block & OnEntry
        var nopTryStart = il.Create(OpCodes.Nop);
        newLayer.Add(nopTryStart);
        newLayer.Add(il.Create(OpCodes.Ldloc, aspectVar));
        newLayer.Add(il.Create(OpCodes.Ldloc, entryContext));
        newLayer.Add(il.Create(OpCodes.Callvirt, refs.OnEntry));

        // 3. Inner Instructions Logic
        foreach (var instruction in innerInstructions)
        {
            if (instruction.OpCode == OpCodes.Ret)
            {
                if (returnVar != null)
                    newLayer.Add(il.Create(OpCodes.Stloc, returnVar));

                newLayer.Add(il.Create(OpCodes.Leave, exitPoint));
            }
            else
            {
                newLayer.Add(instruction);
            }
        }

        // Safety leave if the inner instructions don't end in a Ret
        newLayer.Add(il.Create(OpCodes.Leave, exitPoint));

        // 4. Catch Block
        newLayer.Add(handlerCatchStart);
        newLayer.Add(il.Create(OpCodes.Stloc, exceptionVar));

        // Create ExceptionContext(entry, ex)
        newLayer.Add(il.Create(OpCodes.Ldloc, entryContext));
        newLayer.Add(il.Create(OpCodes.Ldloc, exceptionVar));
        var exCtor = module.ImportReference(typeof(AspectNetExceptionContext).GetConstructor(new[] { typeof(AspectNetEntryContext), typeof(Exception) }));
        newLayer.Add(il.Create(OpCodes.Newobj, exCtor));
        newLayer.Add(il.Create(OpCodes.Stloc, exCtxLocal));

        // Call OnException
        newLayer.Add(il.Create(OpCodes.Ldloc, aspectVar));
        newLayer.Add(il.Create(OpCodes.Ldloc, exCtxLocal));
        newLayer.Add(il.Create(OpCodes.Callvirt, refs.OnException));

        // Rethrow logic
        newLayer.Add(il.Create(OpCodes.Rethrow));

        // 5. Finally Block
        newLayer.Add(handlerFinallyStart);

        // Update the SHARED ExitContext with current ReturnValue before calling OnExit
        if (returnVar != null)
        {
            var setRet = module.ImportReference(typeof(AspectNetExitContext).GetProperty(nameof(AspectNetExitContext.ReturnValue)).SetMethod);
            newLayer.Add(il.Create(OpCodes.Ldloc, exitContext));
            newLayer.Add(il.Create(OpCodes.Ldloc, returnVar));
            if (method.ReturnType.IsValueType) newLayer.Add(il.Create(OpCodes.Box, module.ImportReference(method.ReturnType)));
            newLayer.Add(il.Create(OpCodes.Callvirt, setRet));
        }

        // Call OnExit using the SHARED exitContext
        newLayer.Add(il.Create(OpCodes.Ldloc, aspectVar));
        newLayer.Add(il.Create(OpCodes.Ldloc, exitContext));
        newLayer.Add(il.Create(OpCodes.Callvirt, refs.OnExit));

        // Interceptor Sync: Read possibly modified ReturnValue back from SHARED context
        if (returnVar != null)
        {
            var getRet = module.ImportReference(typeof(AspectNetExitContext).GetProperty(nameof(AspectNetExitContext.ReturnValue)).GetMethod);
            newLayer.Add(il.Create(OpCodes.Ldloc, exitContext));
            newLayer.Add(il.Create(OpCodes.Callvirt, getRet));
            newLayer.Add(il.Create(OpCodes.Unbox_Any, module.ImportReference(method.ReturnType)));
            newLayer.Add(il.Create(OpCodes.Stloc, returnVar));
        }

        newLayer.Add(il.Create(OpCodes.Endfinally));

        // 6. Assembly Exit Point
        newLayer.Add(exitPoint);

        // Register Exception Handlers
        method.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            TryStart = nopTryStart,
            TryEnd = handlerCatchStart,
            HandlerStart = handlerCatchStart,
            HandlerEnd = handlerFinallyStart,
            CatchType = module.ImportReference(typeof(Exception))
        });

        method.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Finally)
        {
            TryStart = nopTryStart,
            TryEnd = handlerFinallyStart,
            HandlerStart = handlerFinallyStart,
            HandlerEnd = exitPoint
        });

        return newLayer.ToArray();
    }
}
