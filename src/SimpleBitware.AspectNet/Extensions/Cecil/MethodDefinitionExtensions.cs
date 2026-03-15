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
            .Where(a => a.AttributeType.Resolve()?.InheritsFrom(baseAspectNetAttribute) ?? false)
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
        if (parentProperty.CustomAttributes.Any(a => filterAttributeFullNames.Any(f => f == a.AttributeType.FullName)))
            return [];

        return parentProperty.CustomAttributes
            .Where(a => a.AttributeType.Resolve()?.InheritsFrom(baseAspectNetAttribute) ?? false)
            .ToArray();
    }

    public static MethodDefinition WeaveMethod(
        this KeyValuePair<MethodDefinition, CustomAttribute[]> methodWithAspects,
        AspectReferences aspectNetReferences)
    {
        var method = methodWithAspects.Key;
        var attributes = methodWithAspects.Value;

        attributes
            .OrderBy(a => a.GetPriorityValue())
            .Reverse()
            .Each(x =>
            {
                var aspectVar = method.InstantiateAspectInLocals(x);
                method.WeaveWithContextAndReturn(
                    x,
                    aspectNetReferences,
                    method.Body.Instructions.ToList()
                );
            });

        return method;
    }

    public static VariableDefinition InstantiateAspectInLocals(this MethodDefinition method, CustomAttribute attribute)
    {
        var processor = method.Body.GetILProcessor();
        var module = method.Module;

        // 1. Create a local variable to hold the aspect instance
        var aspectType = module.ImportReference(attribute.AttributeType);
        var aspectVar = new VariableDefinition(aspectType);
        method.Body.Variables.Add(aspectVar);

        // 2. Push Constructor Arguments onto the stack
        foreach (var arg in attribute.ConstructorArguments)
        {
            processor.PushValue(arg.Value, arg.Type);
        }

        // 3. Instantiate the object
        var ctor = module.ImportReference(attribute.Constructor);
        processor.Append(processor.Create(OpCodes.Newobj, ctor));
        processor.Append(processor.Create(OpCodes.Stloc, aspectVar));

        // 4. Handle Named Arguments (Properties like Priority)
        foreach (var prop in attribute.Properties)
        {
            processor.Append(processor.Create(OpCodes.Ldloc, aspectVar));
            processor.PushValue(prop.Argument.Value, prop.Argument.Type);

            // Resolve the setter for the property
            var typeDef = attribute.AttributeType.Resolve();
            var propertyDef = typeDef.Properties.First(p => p.Name == prop.Name);
            var setter = module.ImportReference(propertyDef.SetMethod);

            processor.Append(processor.Create(OpCodes.Callvirt, setter));
        }

        return aspectVar;
    }

    public static MethodDefinition ApplyMarkerAttribute(this MethodDefinition method, MethodReference? markerAttributeConstructor)
    {
        if (markerAttributeConstructor != null)
            method.CustomAttributes.Add(new CustomAttribute(markerAttributeConstructor));

        return method;
    }

    public static void Optimize(this MethodDefinition method)
    {
        method.Body.OptimizeMacros();
    }

    public static void WeaveWithContextAndReturn(
        this MethodDefinition method,
        CustomAttribute aspectAttribute, // Pass the attribute metadata directly
        AspectReferences aspectReferences,
        List<Instruction> body)
    {
        var il = method.Body.GetILProcessor();
        var module = method.Module;

        // 1. Identify Logic Flags
        bool isVoid = method.ReturnType.MetadataType == MetadataType.Void || method.IsConstructor;
        bool isAsync = method.CustomAttributes.Any(a => a.AttributeType.Name == "AsyncStateMachineAttribute");

        // 2. Resolve Metadata References
        var exitContextType = module.ImportReference(typeof(AspectNetExitContext));
        var exceptionContextType = module.ImportReference(typeof(AspectNetExceptionContext));
        var exceptionType = module.ImportReference(typeof(Exception));
        var dictCtor = module.ImportReference(typeof(Dictionary<string, object>).GetConstructor(Type.EmptyTypes));
        var dictAdd = module.ImportReference(typeof(Dictionary<string, object>).GetMethod("Add", new[] { typeof(string), typeof(object) }));
        var getParams = module.ImportReference(typeof(AspectNetExitContext).GetProperty("Parameters").GetMethod);
        var setParams = module.ImportReference(typeof(AspectNetExitContext).GetProperty("Parameters").SetMethod);

        // 3. Setup Local Variables
        // The Aspect Instance
        var aspectType = module.ImportReference(aspectAttribute.AttributeType);
        var aspectVar = new VariableDefinition(aspectType);
        method.Body.Variables.Add(aspectVar);

        // The Contexts
        var contextVar = new VariableDefinition(exitContextType);
        var exceptionVar = new VariableDefinition(exceptionType);
        method.Body.Variables.Add(contextVar);
        method.Body.Variables.Add(exceptionVar);

        // Return Value holder
        VariableDefinition? returnVariable = isVoid ? null : new VariableDefinition(method.ReturnType);
        if (returnVariable != null) method.Body.Variables.Add(returnVariable);

        // Jump Targets
        var nopTryStart = il.Create(OpCodes.Nop);
        var handlerCatchStart = il.Create(OpCodes.Stloc, exceptionVar);
        var handlerFinallyStart = il.Create(OpCodes.Nop);
        var exitPoint = il.Create(OpCodes.Nop);

        // 4. Split for Constructor Safety
        // We isolate the base() call so we don't weave logic before the object exists.
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

        // 5. REBUILD THE METHOD BODY
        method.Body.Instructions.Clear();

        // A. Prologue (e.g., ldarg.0, call base::.ctor)
        foreach (var instr in prologue) il.Append(instr);

        // B. ASPECT INITIALIZATION (Crucial Fix for NullReferenceException)
        // We create the aspect and store it in the local variable immediately
        il.Append(il.Create(OpCodes.Newobj, module.ImportReference(aspectAttribute.Constructor)));
        il.Append(il.Create(OpCodes.Stloc, aspectVar));

        // C. CONTEXT INITIALIZATION
        il.Append(il.Create(OpCodes.Newobj, module.ImportReference(typeof(AspectNetExitContext).GetConstructor(Type.EmptyTypes))));
        il.Append(il.Create(OpCodes.Stloc, contextVar));

        // Metadata: Class & Member Names
        var setClassName = module.ImportReference(typeof(AspectNetExitContext).GetProperty("ClassName").SetMethod);
        var setMemberName = module.ImportReference(typeof(AspectNetExitContext).GetProperty("MemberName").SetMethod);

        il.Append(il.Create(OpCodes.Ldloc, contextVar));
        il.Append(il.Create(OpCodes.Ldstr, method.DeclaringType.FullName));
        il.Append(il.Create(OpCodes.Callvirt, setClassName));

        il.Append(il.Create(OpCodes.Ldloc, contextVar));
        il.Append(il.Create(OpCodes.Ldstr, method.Name));
        il.Append(il.Create(OpCodes.Callvirt, setMemberName));

        // Initialize Parameters Dictionary
        il.Append(il.Create(OpCodes.Ldloc, contextVar));
        il.Append(il.Create(OpCodes.Newobj, dictCtor));
        il.Append(il.Create(OpCodes.Callvirt, setParams));

        foreach (var param in method.Parameters)
        {
            il.Append(il.Create(OpCodes.Ldloc, contextVar));
            il.Append(il.Create(OpCodes.Callvirt, getParams));
            il.Append(il.Create(OpCodes.Ldstr, param.Name));
            il.Append(il.Create(OpCodes.Ldarg, param));

            if (param.ParameterType.IsValueType || param.ParameterType is GenericParameter)
                il.Append(il.Create(OpCodes.Box, param.ParameterType));

            il.Append(il.Create(OpCodes.Callvirt, dictAdd));
        }

        // D. TRY BLOCK START
        il.Append(nopTryStart);

        // OnEntry
        il.Append(il.Create(OpCodes.Ldloc, aspectVar));
        il.Append(il.Create(OpCodes.Ldloc, contextVar));
        il.Append(il.Create(OpCodes.Callvirt, aspectReferences.OnEntry));

        // Original Body (Replace Ret with Leave)
        foreach (var instr in body)
        {
            if (instr.OpCode == OpCodes.Ret)
            {
                if (returnVariable != null) il.Append(il.Create(OpCodes.Stloc, returnVariable));
                il.Append(il.Create(OpCodes.Leave, exitPoint));
            }
            else il.Append(instr);
        }

        il.Append(il.Create(OpCodes.Leave, exitPoint));

        // E. CATCH BLOCK
        il.Append(handlerCatchStart);

        var exContextVar = new VariableDefinition(exceptionContextType);
        method.Body.Variables.Add(exContextVar);
        il.Append(il.Create(OpCodes.Newobj, module.ImportReference(typeof(AspectNetExceptionContext).GetConstructor(Type.EmptyTypes))));
        il.Append(il.Create(OpCodes.Stloc, exContextVar));

        var setEx = module.ImportReference(typeof(AspectNetExceptionContext).GetProperty("Exception").SetMethod);
        il.Append(il.Create(OpCodes.Ldloc, exContextVar));
        il.Append(il.Create(OpCodes.Ldloc, exceptionVar));
        il.Append(il.Create(OpCodes.Callvirt, setEx));

        il.Append(il.Create(OpCodes.Ldloc, aspectVar));
        il.Append(il.Create(OpCodes.Ldloc, exContextVar));
        il.Append(il.Create(OpCodes.Callvirt, aspectReferences.OnException));
        il.Append(il.Create(OpCodes.Rethrow));

        // F. FINALLY BLOCK
        il.Append(handlerFinallyStart);

        if (returnVariable != null)
        {
            var setRet = module.ImportReference(typeof(AspectNetExitContext).GetProperty("ReturnValue").SetMethod);
            il.Append(il.Create(OpCodes.Ldloc, contextVar));
            il.Append(il.Create(OpCodes.Ldloc, returnVariable));
            if (method.ReturnType.IsValueType) il.Append(il.Create(OpCodes.Box, method.ReturnType));
            il.Append(il.Create(OpCodes.Callvirt, setRet));
        }

        // OnExit
        il.Append(il.Create(OpCodes.Ldloc, aspectVar));
        il.Append(il.Create(OpCodes.Ldloc, contextVar));
        il.Append(il.Create(OpCodes.Callvirt, aspectReferences.OnExit));
        il.Append(il.Create(OpCodes.Endfinally));

        // G. EXIT POINT
        il.Append(exitPoint);
        if (returnVariable != null) il.Append(il.Create(OpCodes.Ldloc, returnVariable));
        il.Append(il.Create(OpCodes.Ret));

        // H. EXCEPTION HANDLER REGISTRATION
        method.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            TryStart = nopTryStart,
            TryEnd = handlerCatchStart,
            HandlerStart = handlerCatchStart,
            HandlerEnd = handlerFinallyStart,
            CatchType = exceptionType
        });

        method.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Finally)
        {
            TryStart = nopTryStart,
            TryEnd = handlerFinallyStart,
            HandlerStart = handlerFinallyStart,
            HandlerEnd = exitPoint
        });

        method.Body.OptimizeMacros();
    }

    //--------------------------------------

    // public static void WeaveWithContextAndReturn(
    //     this MethodDefinition method,
    //     VariableDefinition aspectVar,
    //     MethodReference onEntry,
    //     MethodReference onException,
    //     MethodReference onExit,
    //     Instruction[] originalInstructions)
    // {
    //     var il = method.Body.GetILProcessor();
    //     var module = method.Module;
    //
    //     // 1. Resolve Types
    //     var returnContextType = module.ImportReference(typeof(AspectNetExitContext));
    //     var exceptionContextType = module.ImportReference(typeof(AspectNetExceptionContext));
    //     var exceptionType = module.ImportReference(typeof(Exception));
    //
    //     // 2. Determine Method Kind
    //     bool isAsync = method.CustomAttributes.Any(a => a.AttributeType.Name == "AsyncStateMachineAttribute");
    //     bool isVoid = method.ReturnType.MetadataType == MetadataType.Void;
    //
    //     // 3. Setup Locals
    //     var contextVar = new VariableDefinition(returnContextType);
    //     method.Body.Variables.Add(contextVar);
    //
    //     var exceptionVar = new VariableDefinition(exceptionType);
    //     method.Body.Variables.Add(exceptionVar);
    //
    //     VariableDefinition? returnVariable = isVoid ? null : new VariableDefinition(method.ReturnType);
    //     if (returnVariable != null) method.Body.Variables.Add(returnVariable);
    //
    //     // Markers
    //     var nopTryStart = Instruction.Create(OpCodes.Nop);
    //     var handlerCatchStart = Instruction.Create(OpCodes.Stloc, exceptionVar);
    //     var handlerFinallyStart = Instruction.Create(OpCodes.Nop);
    //     var exitPoint = Instruction.Create(OpCodes.Nop);
    //
    //     // --- A. INITIALIZE CONTEXT ---
    //     il.Append(il.Create(OpCodes.Newobj, module.ImportReference(typeof(AspectNetExitContext).GetConstructor(Type.EmptyTypes))));
    //     il.Append(il.Create(OpCodes.Stloc, contextVar));
    //
    //     SetProperty<AspectNetExitContext>(il, contextVar, "ClassName", il.Create(OpCodes.Ldstr, method.DeclaringType.FullName));
    //     SetProperty<AspectNetExitContext>(il, contextVar, "MemberName", il.Create(OpCodes.Ldstr, method.Name));
    //
    //     // Initialize Parameters Dictionary
    //     var dictCtor = module.ImportReference(typeof(Dictionary<string, object>).GetConstructor(Type.EmptyTypes));
    //     var setParams = module.ImportReference(typeof(AspectNetEntryContext).GetProperty("Parameters").SetMethod);
    //     il.Append(il.Create(OpCodes.Ldloc, contextVar));
    //     il.Append(il.Create(OpCodes.Newobj, dictCtor));
    //     il.Append(il.Create(OpCodes.Callvirt, setParams));
    //
    //     // --- B. TRY BLOCK START ---
    //     il.Append(nopTryStart);
    //
    //     // OnEntry
    //     il.Append(il.Create(OpCodes.Ldloc, aspectVar));
    //     il.Append(il.Create(OpCodes.Ldloc, contextVar));
    //     il.Append(il.Create(OpCodes.Callvirt, onEntry));
    //
    //     // Original Body
    //     foreach (var instr in originalInstructions)
    //     {
    //         if (instr.OpCode == OpCodes.Ret)
    //         {
    //             if (returnVariable != null) il.Append(il.Create(OpCodes.Stloc, returnVariable));
    //             il.Append(il.Create(OpCodes.Leave, exitPoint));
    //         }
    //         else il.Append(instr);
    //     }
    //
    //     il.Append(il.Create(OpCodes.Leave, exitPoint));
    //
    //     // --- C. CATCH BLOCK (Sync Exceptions) ---
    //     il.Append(handlerCatchStart);
    //
    //     var exContextVar = new VariableDefinition(exceptionContextType);
    //     method.Body.Variables.Add(exContextVar);
    //     il.Append(il.Create(OpCodes.Newobj, module.ImportReference(typeof(AspectNetExceptionContext).GetConstructor(Type.EmptyTypes))));
    //     il.Append(il.Create(OpCodes.Stloc, exContextVar));
    //
    //     // Set Exception property and call OnException
    //     SetProperty<AspectNetExceptionContext>(il, exContextVar, "Exception", il.Create(OpCodes.Ldloc, exceptionVar));
    //     il.Append(il.Create(OpCodes.Ldloc, aspectVar));
    //     il.Append(il.Create(OpCodes.Ldloc, exContextVar));
    //     il.Append(il.Create(OpCodes.Callvirt, onException));
    //     il.Append(il.Create(OpCodes.Leave, exitPoint));
    //
    //     // --- D. FINALLY BLOCK ---
    //     il.Append(handlerFinallyStart);
    //
    //     if (returnVariable != null)
    //     {
    //         // Set ReturnValue
    //         il.Append(il.Create(OpCodes.Ldloc, contextVar));
    //         il.Append(il.Create(OpCodes.Ldloc, returnVariable));
    //         if (method.ReturnType.IsValueType) il.Append(il.Create(OpCodes.Box, method.ReturnType));
    //         il.Append(il.Create(OpCodes.Callvirt, module.ImportReference(typeof(AspectNetExitContext).GetProperty("ReturnValue").SetMethod)));
    //
    //         // ASYNC HANDLING: If it's a Task, hook the continuation
    //         if (isAsync || method.ReturnType.Name.Contains("Task"))
    //         {
    //             var handleAsync = module.ImportReference(typeof(AspectNetRuntime).GetMethod("HandleAsyncExtension"));
    //             il.Append(il.Create(OpCodes.Ldloc, aspectVar));
    //             il.Append(il.Create(OpCodes.Ldloc, contextVar));
    //             il.Append(il.Create(OpCodes.Call, handleAsync));
    //         }
    //     }
    //
    //     // OnExit
    //     il.Append(il.Create(OpCodes.Ldloc, aspectVar));
    //     il.Append(il.Create(OpCodes.Ldloc, contextVar));
    //     il.Append(il.Create(OpCodes.Callvirt, onExit));
    //     il.Append(il.Create(OpCodes.Endfinally));
    //
    //     // --- E. RETURN ---
    //     il.Append(exitPoint);
    //     if (returnVariable != null) il.Append(il.Create(OpCodes.Ldloc, returnVariable));
    //     il.Append(il.Create(OpCodes.Ret));
    //
    //     AddHandlers(method, nopTryStart, handlerCatchStart, handlerFinallyStart, exitPoint, exceptionType);
    //     method.Body.OptimizeMacros();
    // }

    private static void SetProperty<T>(ILProcessor il, VariableDefinition local, string propName, Instruction loadValue)
    {
        var module = il.Body.Method.Module;

        // 1. Use Reflection on the actual Type to find the setter
        // This is more reliable than Cecil's Resolve() for external types
        var propertyInfo = typeof(T).GetProperty(propName);
        if (propertyInfo == null || propertyInfo.SetMethod == null)
            throw new Exception($"Property {propName} or its setter not found on AspectContext");

        // 2. Import the setter method into the current module
        var setterReference = module.ImportReference(propertyInfo.SetMethod);

        // 3. Emit the IL
        il.Append(il.Create(OpCodes.Ldloc, local)); // Load the Context instance
        il.Append(loadValue); // Load the value (e.g. Ldstr "MethodName")
        il.Append(il.Create(OpCodes.Callvirt, setterReference));
    }

    private static void AddHandlers(
        MethodDefinition method,
        Instruction tryStart,
        Instruction catchStart,
        Instruction finallyStart,
        Instruction exitPoint,
        TypeReference exceptionType)
    {
        // Catch Handler
        var catchHandler = new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            CatchType = exceptionType,
            TryStart = tryStart,
            TryEnd = catchStart,
            HandlerStart = catchStart,
            HandlerEnd = finallyStart
        };

        // Finally Handler
        var finallyHandler = new ExceptionHandler(ExceptionHandlerType.Finally)
        {
            TryStart = tryStart,
            TryEnd = finallyStart,
            HandlerStart = finallyStart,
            HandlerEnd = exitPoint
        };

        method.Body.ExceptionHandlers.Add(catchHandler);
        method.Body.ExceptionHandlers.Add(finallyHandler);
    }
}
