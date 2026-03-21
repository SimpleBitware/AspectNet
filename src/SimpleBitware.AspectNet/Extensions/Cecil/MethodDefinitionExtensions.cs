using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using SimpleBitware.AspectNet.Abstractions;
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
            .Where(customAttribute => customAttribute.AttributeType.Resolve()?.InheritsFrom(baseAspectNetAttribute) ?? false)
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

// Helper to handle generic method creation in Cecil
    public static MethodReference MakeGeneric(this MethodReference method, params TypeReference[] args)
    {
        var genericType = new GenericInstanceMethod(method);
        foreach (var arg in args) genericType.GenericArguments.Add(arg);
        return genericType;
    }

    public static Instruction[] GetMethodInstructions(this MethodDefinition method)
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
    
    public static Instruction[] GetMethodProlog(this MethodDefinition method)
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

    public static void ClearMethodBody(this MethodDefinition method)
    {
        method.Body.Instructions.Clear();
        method.Body.ExceptionHandlers.Clear();
    }

    /// --------------
    ///
    ///
    public static MethodDefinition WeaveMethod<TContext>(this KeyValuePair<MethodDefinition, CustomAttribute[]> methodWithAspects)
    {
        var method = methodWithAspects.Key;
        var aspectAttributes = methodWithAspects.Value;
        var il = method.Body.GetILProcessor();
        var module = method.Module;

        // 2. Extract Original Instructions & Prologue
        var prologue = method.GetMethodProlog();
        var workingBody = method.GetMethodInstructions();

        method.ClearMethodBody();

        // --- A. EMIT PROLOGUE ---
        prologue.Each(instruction => il.Append(instruction));

        var entryContextVar = new VariableDefinition(module.ImportReference(typeof(TContext)));
        il.CreateEntryContext<TContext>(module, entryContextVar, method);
        method.Body.Variables.Add(entryContextVar);

        // --- C. RECURSIVE WRAP ---
        aspectAttributes
            .OrderBy(customAttribute => customAttribute.GetPriorityValue())
            .Reverse()
            .Each(attribute =>
            {
                workingBody = WrapInAttributeLayer(method, attribute, workingBody.ToArray(), entryContextVar);
                method.CustomAttributes.Remove(attribute);
            });
        
        // --- D. FINAL ASSEMBLY ---
        workingBody.Each(instruction => il.Append(instruction));

        il.AddMethodReturn(method);

        return method;
    }

    private static Instruction[] WrapInAttributeLayer(
        MethodDefinition method,
        CustomAttribute attr,
        Instruction[] innerInstructions,
        VariableDefinition entryContext)
    {
        var il = method.Body.GetILProcessor();
        var module = method.Module;
        var isInterceptor = attr.AttributeType.Resolve().DerivesFrom("AbstractInterceptorAttribute");
        var refs = new AspectReferences(module, attr.AttributeType.Resolve());
        var isVoid = method.ReturnType.MetadataType == MetadataType.Void || method.IsConstructor;

        // Layer Locals
        var aspectVar = new VariableDefinition(module.ImportReference(attr.AttributeType));
        var exceptionVar = new VariableDefinition(module.ImportReference(typeof(Exception)));
        method.Body.Variables.Add(aspectVar);
        method.Body.Variables.Add(exceptionVar);

        // Find or Create Return Variable
        VariableDefinition? returnVar = isVoid ? null : method.Body.Variables.FirstOrDefault(v => v.VariableType.FullName == method.ReturnType.FullName);
        if (!isVoid && returnVar == null)
        {
            returnVar = new VariableDefinition(method.ReturnType);
            method.Body.Variables.Add(returnVar);
        }

        // Jump Targets
        var nopTryStart = il.Create(OpCodes.Nop);
        var handlerCatchStart = il.Create(OpCodes.Stloc, exceptionVar);
        var handlerFinallyStart = il.Create(OpCodes.Nop);
        var exitPoint = il.Create(OpCodes.Nop);

        List<Instruction> newLayer = new();

        // 1. Get Aspect Instance
        var getService = module.ImportReference(typeof(AspectNetDependencyInjection).GetMethod("GetRequiredService"))
            .MakeGeneric(aspectVar.VariableType);
        newLayer.Add(il.Create(OpCodes.Call, getService));
        newLayer.Add(il.Create(OpCodes.Stloc, aspectVar));

        // 2. Start Try Block & OnEntry
        newLayer.Add(nopTryStart);
        newLayer.Add(il.Create(OpCodes.Ldloc, aspectVar));
        newLayer.Add(il.Create(OpCodes.Ldloc, entryContext));
        newLayer.Add(il.Create(OpCodes.Callvirt, refs.OnEntry));

        // 3. Inner Instructions
        foreach (var instr in innerInstructions)
        {
            if (instr.OpCode == OpCodes.Ret)
            {
                // Don't append the Ret!
                if (returnVar != null) newLayer.Add(il.Create(OpCodes.Stloc, returnVar));
                newLayer.Add(il.Create(OpCodes.Leave, exitPoint));
            }
            else
            {
                newLayer.Add(instr);
            }
        }

        newLayer.Add(il.Create(OpCodes.Leave, exitPoint));

        // 4. Catch Block
        newLayer.Add(handlerCatchStart);

        // Create ExceptionContext
        newLayer.Add(il.Create(OpCodes.Ldloc, entryContext));
        newLayer.Add(il.Create(OpCodes.Ldloc, exceptionVar));
        var exCtor = module.ImportReference(typeof(AspectNetExceptionContext).GetConstructor(new[] { typeof(AspectNetEntryContext), typeof(Exception) }));
        newLayer.Add(il.Create(OpCodes.Newobj, exCtor));

        var exCtxLocal = new VariableDefinition(module.ImportReference(typeof(AspectNetExceptionContext)));
        method.Body.Variables.Add(exCtxLocal);
        newLayer.Add(il.Create(OpCodes.Stloc, exCtxLocal));

        // Call OnException
        newLayer.Add(il.Create(OpCodes.Ldloc, aspectVar));
        newLayer.Add(il.Create(OpCodes.Ldloc, exCtxLocal));
        newLayer.Add(il.Create(OpCodes.Callvirt, refs.OnException));
        newLayer.Add(il.Create(OpCodes.Rethrow));

        // 5. Finally Block
        newLayer.Add(handlerFinallyStart);

        // Create ExitContext
        newLayer.Add(il.Create(OpCodes.Ldloc, entryContext));
        if (returnVar != null)
        {
            newLayer.Add(il.Create(OpCodes.Ldloc, returnVar));
            if (method.ReturnType.IsValueType) newLayer.Add(il.Create(OpCodes.Box, method.ReturnType));
        }
        else newLayer.Add(il.Create(OpCodes.Ldnull));

        var exitCtor = module.ImportReference(typeof(AspectNetExitContext).GetConstructor(new[] { typeof(AspectNetEntryContext), typeof(object) }));
        newLayer.Add(il.Create(OpCodes.Newobj, exitCtor));

        var exitCtxLocal = new VariableDefinition(module.ImportReference(typeof(AspectNetExitContext)));
        method.Body.Variables.Add(exitCtxLocal);
        newLayer.Add(il.Create(OpCodes.Stloc, exitCtxLocal));

        // Call OnExit
        newLayer.Add(il.Create(OpCodes.Ldloc, aspectVar));
        newLayer.Add(il.Create(OpCodes.Ldloc, exitCtxLocal));
        newLayer.Add(il.Create(OpCodes.Callvirt, refs.OnExit));

        // Interceptor Sync
        if (isInterceptor && returnVar != null)
        {
            var getRet = module.ImportReference(typeof(AspectNetExitContext).GetProperty("ReturnValue").GetMethod);
            newLayer.Add(il.Create(OpCodes.Ldloc, exitCtxLocal));
            newLayer.Add(il.Create(OpCodes.Callvirt, getRet));
            newLayer.Add(il.Create(OpCodes.Unbox_Any, method.ReturnType));
            newLayer.Add(il.Create(OpCodes.Stloc, returnVar));
        }

        newLayer.Add(il.Create(OpCodes.Endfinally));

        // 6. Assembly
        newLayer.Add(exitPoint);

        // Handlers
        method.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            TryStart = nopTryStart, TryEnd = handlerCatchStart, HandlerStart = handlerCatchStart, HandlerEnd = handlerFinallyStart, CatchType = module.ImportReference(typeof(Exception))
        });
        method.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Finally)
        {
            TryStart = nopTryStart, TryEnd = handlerFinallyStart, HandlerStart = handlerFinallyStart, HandlerEnd = exitPoint
        });

        return newLayer.ToArray();
    }
}
