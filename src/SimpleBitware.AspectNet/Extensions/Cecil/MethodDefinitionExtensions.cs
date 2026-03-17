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
    /// Gets Property-level aspect derived attributes
    /// </summary>
    /// <param name="method"></param>
    /// <param name="properties"></param>
    /// <param name="baseAspectNetAttribute"></param>
    /// <param name="filterAttributeFullNames"></param>
    /// <returns></returns>
    public static CustomAttribute[] GetPropertyAspectNetDerivedAttributes(
        this MethodDefinition method,
        PropertyDefinition[] properties, TypeDefinition baseAspectNetAttribute,
        string[] filterAttributeFullNames)
    {
        var parentProperty = properties.FirstOrDefault(p => p.GetMethod == method || p.SetMethod == method);
        if (parentProperty == null)
            return [];

        // If property is excluded, skip its accessors entirely
        var propertyIsExcluded = parentProperty.CustomAttributes
            .Any(customAttribute => filterAttributeFullNames
                .Any(filterAttributeFullName => filterAttributeFullName == customAttribute.AttributeType.FullName)
            );
        if (propertyIsExcluded)
            return [];

        return parentProperty.CustomAttributes
            .Where(customAttribute => customAttribute.AttributeType.Resolve()?.InheritsFrom(baseAspectNetAttribute) ?? false)
            .ToArray();
    }

    public static MethodDefinition WeaveMethod(
        this KeyValuePair<MethodDefinition, CustomAttribute[]> methodWithAspects,
        AspectReferences aspectNetReferences)
    {
        var method = methodWithAspects.Key;
        var attributes = methodWithAspects.Value;

        attributes
            .OrderBy(customAttribute => customAttribute.GetPriorityValue())
            .Reverse()
            .Each(customAttribute =>
            {
                //var aspectVar = method.InstantiateAspectInLocals(customAttribute);
                method.WeaveMethodWithAttribute(
                        customAttribute,
                        aspectNetReferences,
                        method.Body.Instructions.ToList()
                    )
                    .Optimize();
            });

        return method;
    }

    public static void ApplyMarkerAttribute(this MethodDefinition method, MethodReference? markerAttributeConstructor)
    {
        //if (markerAttributeConstructor != null)
        method.CustomAttributes.Add(new CustomAttribute(markerAttributeConstructor));
    }

    public static void Optimize(this MethodDefinition method)
    {
        method.Body.OptimizeMacros();
    }

    public static MethodDefinition WeaveMethodWithAttribute(
        this MethodDefinition method,
        CustomAttribute aspectAttribute,
        AspectReferences aspectReferences,
        List<Instruction> body)
    {
        var il = method.Body.GetILProcessor();
        var module = method.Module;

        // 1. Logic Flags
        bool isVoid = method.ReturnType.MetadataType == MetadataType.Void || method.IsConstructor;
        bool isAsync = method.CustomAttributes.Any(a => a.AttributeType.Name == "AsyncStateMachineAttribute");

        // 2. Resolve Metadata References
        var entryContextType = module.ImportReference(typeof(AspectNetEntryContext));
        var exitContextType = module.ImportReference(typeof(AspectNetExitContext));
        var exceptionContextType = module.ImportReference(typeof(AspectNetExceptionContext));
        var exceptionType = module.ImportReference(typeof(Exception));

        // Constructors
        var entryCtor = module.ImportReference(typeof(AspectNetEntryContext).GetConstructor(Type.EmptyTypes));
        var exitCtor = module.ImportReference(typeof(AspectNetExitContext).GetConstructor(new[] { typeof(AspectNetEntryContext), typeof(object) }));
        var exCtor = module.ImportReference(typeof(AspectNetExceptionContext).GetConstructor(new[] { typeof(AspectNetEntryContext), typeof(Exception) }));

        // Dictionary/Property Helpers
        var dictCtor = module.ImportReference(typeof(Dictionary<string, object>).GetConstructor(Type.EmptyTypes));
        var dictAdd = module.ImportReference(typeof(Dictionary<string, object>).GetMethod("Add", new[] { typeof(string), typeof(object) }));
        var getParams = module.ImportReference(typeof(AspectNetEntryContext).GetProperty(nameof(AspectNetEntryContext.Parameters)).GetMethod);
        var setParams = module.ImportReference(typeof(AspectNetEntryContext).GetProperty(nameof(AspectNetEntryContext.Parameters)).SetMethod);
        var setClassName = module.ImportReference(typeof(AspectNetEntryContext).GetProperty(nameof(AspectNetEntryContext.ClassName)).SetMethod);
        var setMemberName = module.ImportReference(typeof(AspectNetEntryContext).GetProperty(nameof(AspectNetEntryContext.MemberName)).SetMethod);

        // 3. Setup Local Variables
        var aspectType = module.ImportReference(aspectAttribute.AttributeType);
        var aspectVar = new VariableDefinition(aspectType);
        var entryContextVar = new VariableDefinition(entryContextType); // Persistent across blocks
        var exceptionVar = new VariableDefinition(exceptionType);

        method.Body.Variables.Add(aspectVar);
        method.Body.Variables.Add(entryContextVar);
        method.Body.Variables.Add(exceptionVar);

        VariableDefinition? returnVariable = isVoid ? null : new VariableDefinition(method.ReturnType);
        if (returnVariable != null) method.Body.Variables.Add(returnVariable);

        // Jump Targets
        var nopTryStart = il.Create(OpCodes.Nop);
        var handlerCatchStart = il.Create(OpCodes.Stloc, exceptionVar);
        var handlerFinallyStart = il.Create(OpCodes.Nop);
        var exitPoint = il.Create(OpCodes.Nop);

        // 4. Constructor Safety Split
        List<Instruction> prologue = new();
        if (method.IsConstructor)
        {
            var baseCall = body.FirstOrDefault(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference mr && mr.Name == ".ctor");
            if (baseCall != null)
            {
                int index = body.IndexOf(baseCall);
                prologue = body.Take(index + 1).ToList();
                body = body.Skip(index + 1).ToList();
            }
        }

        // 5. REBUILD BODY
        method.Body.Instructions.Clear();
        foreach (var instr in prologue) il.Append(instr);

        // A. Aspect DI Initialization
        var getServiceMethod = typeof(AspectNetDependencyInjection).GetMethod(nameof(AspectNetDependencyInjection.GetRequiredService));
        var genericGetService = module.ImportReference(getServiceMethod).MakeGeneric(aspectType);
        il.Emit(OpCodes.Call, genericGetService);
        il.Emit(OpCodes.Stloc, aspectVar);

        // B. Entry Context Initialization
        il.Emit(OpCodes.Newobj, entryCtor);
        il.Emit(OpCodes.Stloc, entryContextVar);

        // Set Context Metadata
        il.Emit(OpCodes.Ldloc, entryContextVar);
        il.Emit(OpCodes.Ldstr, method.DeclaringType.FullName);
        il.Emit(OpCodes.Callvirt, setClassName);
        il.Emit(OpCodes.Ldloc, entryContextVar);
        il.Emit(OpCodes.Ldstr, method.Name);
        il.Emit(OpCodes.Callvirt, setMemberName);

        // Params Dictionary
        il.Emit(OpCodes.Ldloc, entryContextVar);
        il.Emit(OpCodes.Newobj, dictCtor);
        il.Emit(OpCodes.Callvirt, setParams);

        foreach (var param in method.Parameters)
        {
            il.Emit(OpCodes.Ldloc, entryContextVar);
            il.Emit(OpCodes.Callvirt, getParams);
            il.Emit(OpCodes.Ldstr, param.Name);
            il.Emit(OpCodes.Ldarg, param);
            if (param.ParameterType.IsValueType || param.ParameterType is GenericParameter)
                il.Emit(OpCodes.Box, param.ParameterType);
            il.Emit(OpCodes.Callvirt, dictAdd);
        }

        // C. TRY BLOCK
        il.Append(nopTryStart);
        il.Emit(OpCodes.Ldloc, aspectVar);
        il.Emit(OpCodes.Ldloc, entryContextVar);
        il.Emit(OpCodes.Callvirt, aspectReferences.OnEntry);

        foreach (var instr in body)
        {
            if (instr.OpCode == OpCodes.Ret)
            {
                if (returnVariable != null) il.Emit(OpCodes.Stloc, returnVariable);
                il.Emit(OpCodes.Leave, exitPoint);
            }
            else il.Append(instr);
        }

        il.Emit(OpCodes.Leave, exitPoint);

        // D. CATCH BLOCK
        il.Append(handlerCatchStart);

        // Instantiate ExceptionContext(entryContext, exception)
        il.Emit(OpCodes.Ldloc, entryContextVar);
        il.Emit(OpCodes.Ldloc, exceptionVar);
        il.Emit(OpCodes.Newobj, exCtor);
        var exContextVar = new VariableDefinition(exceptionContextType);
        method.Body.Variables.Add(exContextVar);
        il.Emit(OpCodes.Stloc, exContextVar);

        il.Emit(OpCodes.Ldloc, aspectVar);
        il.Emit(OpCodes.Ldloc, exContextVar);
        il.Emit(OpCodes.Callvirt, aspectReferences.OnException);
        il.Emit(OpCodes.Rethrow);

        // E. FINALLY BLOCK
        il.Append(handlerFinallyStart);

        // Instantiate ExitContext(entryContext, returnValue)
        il.Emit(OpCodes.Ldloc, entryContextVar);
        if (returnVariable != null)
        {
            il.Emit(OpCodes.Ldloc, returnVariable);
            if (method.ReturnType.IsValueType) il.Emit(OpCodes.Box, method.ReturnType);
        }
        else
        {
            il.Emit(OpCodes.Ldnull);
        }

        il.Emit(OpCodes.Newobj, exitCtor);
        var exitContextVar = new VariableDefinition(exitContextType);
        method.Body.Variables.Add(exitContextVar);
        il.Emit(OpCodes.Stloc, exitContextVar);

        if (isAsync)
        {
            var handleAsync = module.ImportReference(typeof(AspectNetRuntime).GetMethod("HandleAsyncExtension"));
            il.Emit(OpCodes.Ldloc, aspectVar);
            il.Emit(OpCodes.Ldloc, exitContextVar);
            il.Emit(OpCodes.Call, handleAsync);
        }

        il.Emit(OpCodes.Ldloc, aspectVar);
        il.Emit(OpCodes.Ldloc, exitContextVar);
        il.Emit(OpCodes.Callvirt, aspectReferences.OnExit);
        il.Emit(OpCodes.Endfinally);

        // F. FINAL EXIT
        il.Append(exitPoint);
        if (returnVariable != null) il.Emit(OpCodes.Ldloc, returnVariable);
        il.Emit(OpCodes.Ret);

        // G. REGISTRATION
        method.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            TryStart = nopTryStart, TryEnd = handlerCatchStart,
            HandlerStart = handlerCatchStart, HandlerEnd = handlerFinallyStart,
            CatchType = exceptionType
        });
        method.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Finally)
        {
            TryStart = nopTryStart, TryEnd = handlerFinallyStart,
            HandlerStart = handlerFinallyStart, HandlerEnd = exitPoint
        });

        method.CustomAttributes.Remove(aspectAttribute);
        return method;
    }

// Helper to handle generic method creation in Cecil
    public static MethodReference MakeGeneric(this MethodReference method, params TypeReference[] args)
    {
        var genericType = new GenericInstanceMethod(method);
        foreach (var arg in args) genericType.GenericArguments.Add(arg);
        return genericType;
    }

    /// --------------
    ///
    ///
    public static MethodDefinition WeaveMethod(this KeyValuePair<MethodDefinition, CustomAttribute[]> methodWithAspects)
    {
        var method = methodWithAspects.Key;
        var aspectAttributes = methodWithAspects.Value;

        var il = method.Body.GetILProcessor();
        var module = method.Module;

        // 1. Setup Shared Entry Context (Initialize once at the top)
        var entryContextVar = new VariableDefinition(module.ImportReference(typeof(AspectNetEntryContext)));
        method.Body.Variables.Add(entryContextVar);

        // Extract original instructions and prologue
        var originalInstructions = method.Body.Instructions.ToList();
        List<Instruction> prologue = new();
        List<Instruction> workingBody = originalInstructions;

        if (method.IsConstructor)
        {
            var baseCall = workingBody.FirstOrDefault(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference mr && mr.Name == ".ctor");
            if (baseCall != null)
            {
                int index = workingBody.IndexOf(baseCall);
                prologue = workingBody.Take(index + 1).ToList();
                workingBody = workingBody.Skip(index + 1).ToList();
            }
        }

        // Prepare method body for rebuild
        method.Body.Instructions.Clear();
        method.Body.ExceptionHandlers.Clear();

        // --- A. SHARED PROLOGUE ---
        foreach (var instr in prologue) il.Append(instr);

        // Initialize the SHARED EntryContext once
        il.Emit(OpCodes.Newobj, module.ImportReference(typeof(AspectNetEntryContext).GetConstructor(Type.EmptyTypes)));
        il.Emit(OpCodes.Stloc, entryContextVar);
        // [Insert metadata population for entryContextVar here...]

        // --- B. RECURSIVE WRAPPING ---
        // We process attributes in REVERSE order so the first attribute in the list 
        // becomes the OUTERMOST try block.
        var reversedAttributes = aspectAttributes.AsEnumerable().Reverse().ToList();

        foreach (var attr in reversedAttributes)
        {
            workingBody = WrapInAttributeLayer(method, attr, workingBody, entryContextVar);
        }

        // --- C. FINAL ASSEMBLY ---
        foreach (var instr in workingBody) il.Append(instr);

        // Final Ret handling
        if (workingBody.Last().OpCode != OpCodes.Ret)
        {
            il.Emit(OpCodes.Ret);
        }

        foreach (var attr in aspectAttributes) method.CustomAttributes.Remove(attr);
        method.Body.OptimizeMacros();

        return method;
    }

    private static List<Instruction> WrapInAttributeLayer(
        MethodDefinition method,
        CustomAttribute attr,
        List<Instruction> innerInstructions,
        VariableDefinition entryContext)
    {
        var il = method.Body.GetILProcessor();
        var module = method.Module;
        var isInterceptor = attr.AttributeType.Resolve().DerivesFrom("AbstractInterceptorAttribute");
        var refs = new AspectReferences(module, attr.AttributeType.Resolve());
        var isVoid = method.ReturnType.MetadataType == MetadataType.Void || method.IsConstructor;

        // 1. Setup Layer-Specific Locals
        var aspectVar = new VariableDefinition(module.ImportReference(attr.AttributeType));
        var exceptionVar = new VariableDefinition(module.ImportReference(typeof(Exception)));
        method.Body.Variables.Add(aspectVar);
        method.Body.Variables.Add(exceptionVar);

        // Reuse the existing return variable if it exists, or find it in the method scope
        VariableDefinition? returnVar = isVoid
            ? null
            : method.Body.Variables.FirstOrDefault(v =>
                v.VariableType.FullName == method.ReturnType.FullName);

        // 2. Define Jump Targets
        var nopTryStart = il.Create(OpCodes.Nop);
        var handlerCatchStart = il.Create(OpCodes.Stloc, exceptionVar);
        var handlerFinallyStart = il.Create(OpCodes.Nop);
        var exitPoint = il.Create(OpCodes.Nop);

        List<Instruction> newLayer = new();

        // --- A. INITIALIZE ASPECT ---
        var getService = module.ImportReference(typeof(AspectNetDependencyInjection).GetMethod("GetRequiredService"))
            .MakeGeneric(aspectVar.VariableType);
        newLayer.Add(il.Create(OpCodes.Call, getService));
        newLayer.Add(il.Create(OpCodes.Stloc, aspectVar));

        // --- B. TRY BLOCK START & OnEntry ---
        newLayer.Add(nopTryStart);
        newLayer.Add(il.Create(OpCodes.Ldloc, aspectVar));
        newLayer.Add(il.Create(OpCodes.Ldloc, entryContext));
        newLayer.Add(il.Create(OpCodes.Callvirt, refs.OnEntry));

        // --- C. INNER CONTENT (Original code or inner aspect) ---
        foreach (var instr in innerInstructions)
        {
            if (instr.OpCode == OpCodes.Ret)
            {
                if (returnVar != null) newLayer.Add(il.Create(OpCodes.Stloc, returnVar));
                newLayer.Add(il.Create(OpCodes.Leave, exitPoint));
            }
            else
            {
                newLayer.Add(instr);
            }
        }

        // Safety leave in case the inner instructions didn't end with a Ret/Leave
        newLayer.Add(il.Create(OpCodes.Leave, exitPoint));

        // --- D. CATCH BLOCK ---
        newLayer.Add(handlerCatchStart);

        // Create ExceptionContext(entryContext, exceptionVar)
        newLayer.Add(il.Create(OpCodes.Ldloc, entryContext));
        newLayer.Add(il.Create(OpCodes.Ldloc, exceptionVar));
        var exCtor = module.ImportReference(typeof(AspectNetExceptionContext).GetConstructor(new[] { typeof(AspectNetEntryContext), typeof(Exception) }));
        newLayer.Add(il.Create(OpCodes.Newobj, exCtor));

        var exCtxLocal = new VariableDefinition(module.ImportReference(typeof(AspectNetExceptionContext)));
        method.Body.Variables.Add(exCtxLocal);
        newLayer.Add(il.Create(OpCodes.Stloc, exCtxLocal));

        // Call OnException: aspect.OnException(exContext)
        newLayer.Add(il.Create(OpCodes.Ldloc, aspectVar));
        newLayer.Add(il.Create(OpCodes.Ldloc, exCtxLocal));
        newLayer.Add(il.Create(OpCodes.Callvirt, refs.OnException));
        newLayer.Add(il.Create(OpCodes.Rethrow));

        // --- E. FINALLY BLOCK ---
        newLayer.Add(handlerFinallyStart);

        // Create ExitContext(entryContext, returnVar ?? null)
        newLayer.Add(il.Create(OpCodes.Ldloc, entryContext));
        if (returnVar != null)
        {
            newLayer.Add(il.Create(OpCodes.Ldloc, returnVar));
            if (method.ReturnType.IsValueType) newLayer.Add(il.Create(OpCodes.Box, method.ReturnType));
        }
        else
        {
            newLayer.Add(il.Create(OpCodes.Ldnull));
        }

        var exitCtor = module.ImportReference(typeof(AspectNetExitContext).GetConstructor(new[] { typeof(AspectNetEntryContext), typeof(object) }));
        newLayer.Add(il.Create(OpCodes.Newobj, exitCtor));

        var exitCtxLocal = new VariableDefinition(module.ImportReference(typeof(AspectNetExitContext)));
        method.Body.Variables.Add(exitCtxLocal);
        newLayer.Add(il.Create(OpCodes.Stloc, exitCtxLocal));

        // Call OnExit: aspect.OnExit(exitContext)
        newLayer.Add(il.Create(OpCodes.Ldloc, aspectVar));
        newLayer.Add(il.Create(OpCodes.Ldloc, exitCtxLocal));
        newLayer.Add(il.Create(OpCodes.Callvirt, refs.OnExit));

        // INTERCEPTOR WRITE-BACK: Only if the attribute is an interceptor
        if (isInterceptor && returnVar != null)
        {
            var getRet = module.ImportReference(typeof(AspectNetExitContext).GetProperty("ReturnValue").GetMethod);
            newLayer.Add(il.Create(OpCodes.Ldloc, exitCtxLocal));
            newLayer.Add(il.Create(OpCodes.Callvirt, getRet));
            newLayer.Add(il.Create(OpCodes.Unbox_Any, method.ReturnType));
            newLayer.Add(il.Create(OpCodes.Stloc, returnVar));
        }

        newLayer.Add(il.Create(OpCodes.Endfinally));

        // --- F. EXIT POINT ---
        newLayer.Add(exitPoint);

        // --- G. REGISTER HANDLERS ---
        var catchHandler = new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            TryStart = nopTryStart,
            TryEnd = handlerCatchStart,
            HandlerStart = handlerCatchStart,
            HandlerEnd = handlerFinallyStart,
            CatchType = module.ImportReference(typeof(Exception))
        };

        var finallyHandler = new ExceptionHandler(ExceptionHandlerType.Finally)
        {
            TryStart = nopTryStart,
            TryEnd = handlerFinallyStart,
            HandlerStart = handlerFinallyStart,
            HandlerEnd = exitPoint
        };

        method.Body.ExceptionHandlers.Add(catchHandler);
        method.Body.ExceptionHandlers.Add(finallyHandler);

        return newLayer;
    }
}
