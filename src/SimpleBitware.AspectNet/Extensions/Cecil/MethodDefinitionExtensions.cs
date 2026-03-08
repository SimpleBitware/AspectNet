using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using SimpleBitware.AspectNet.Abstractions;

namespace SimpleBitware.AspectNet.Extensions.Cecil;

public static class MethodDefinitionExtensions
{
    public static void WeaveWithContextAndReturn(
        this MethodDefinition method,
        VariableDefinition aspectVar,
        MethodReference onEntry,
        MethodReference onException,
        MethodReference onExit,
        List<Instruction> originalInstructions)
    {
        var il = method.Body.GetILProcessor();
        var module = method.Module;

        // 1. Resolve AspectContext References
        var contextType = module.ImportReference(typeof(AspectNetContext));
        var contextCtor = module.ImportReference(typeof(AspectNetContext).GetConstructor(Type.EmptyTypes));
        var dictCtor = module.ImportReference(typeof(Dictionary<string, object>).GetConstructor(Type.EmptyTypes));
        var dictAdd = module.ImportReference(typeof(Dictionary<string, object>).GetMethod("Add"));
        var setReturnValue = module.ImportReference(typeof(AspectNetContext).GetProperty("ReturnValue").SetMethod);

        // 2. Determine Async Status
        bool isAsync = method.CustomAttributes.Any(a => a.AttributeType.Name == "AsyncStateMachineAttribute");
        bool isVoid = method.ReturnType.MetadataType == MetadataType.Void;

        // 3. Setup Locals
        var contextVar = new VariableDefinition(contextType);
        method.Body.Variables.Add(contextVar);
        var exceptionVar = new VariableDefinition(module.ImportReference(typeof(Exception)));
        method.Body.Variables.Add(exceptionVar);

        VariableDefinition returnVariable = isVoid ? null : new VariableDefinition(method.ReturnType);
        if (returnVariable != null) method.Body.Variables.Add(returnVariable);

        // Structural Markers
        var nopTryStart = Instruction.Create(OpCodes.Nop);
        var handlerCatchStart = Instruction.Create(OpCodes.Stloc, exceptionVar);
        var handlerFinallyStart = Instruction.Create(OpCodes.Nop);
        var exitPoint = Instruction.Create(OpCodes.Nop);

        // --- A. CONTEXT INITIALIZATION ---
        il.Append(il.Create(OpCodes.Newobj, contextCtor));
        il.Append(il.Create(OpCodes.Stloc, contextVar));

        // Set Class/Method Name
        SetProperty(il, contextVar, "ClassName", il.Create(OpCodes.Ldstr, method.DeclaringType.FullName));
        SetProperty(il, contextVar, "MemberName", il.Create(OpCodes.Ldstr, method.Name));

        // Initialize Dictionary and Params
        il.Append(il.Create(OpCodes.Ldloc, contextVar));
        il.Append(il.Create(OpCodes.Newobj, dictCtor));
        il.Append(il.Create(OpCodes.Callvirt, module.ImportReference(typeof(AspectNetContext).GetProperty("Parameters").SetMethod)));

        foreach (var param in method.Parameters)
        {
            il.Append(il.Create(OpCodes.Ldloc, contextVar));
            il.Append(il.Create(OpCodes.Callvirt, module.ImportReference(typeof(AspectNetContext).GetProperty("Parameters").GetMethod)));
            il.Append(il.Create(OpCodes.Ldstr, param.Name));
            il.Append(il.Create(OpCodes.Ldarg, param));
            if (param.ParameterType.IsValueType) il.Append(il.Create(OpCodes.Box, param.ParameterType));
            il.Append(il.Create(OpCodes.Callvirt, dictAdd));
        }

        // --- B. START TRY ---
        il.Append(nopTryStart);

        // OnEntry
        il.Append(il.Create(OpCodes.Ldloc, aspectVar));
        il.Append(il.Create(OpCodes.Ldloc, contextVar));
        il.Append(il.Create(OpCodes.Callvirt, onEntry));

        // --- C. ORIGINAL PAYLOAD ---
        foreach (var instr in originalInstructions)
        {
            if (instr.OpCode == OpCodes.Ret)
            {
                if (!isVoid) il.Append(il.Create(OpCodes.Stloc, returnVariable));
                il.Append(il.Create(OpCodes.Leave, exitPoint));
            }
            else il.Append(instr);
        }

        il.Append(il.Create(OpCodes.Leave, exitPoint));

        // --- D. CATCH ---
        il.Append(handlerCatchStart);
        il.Append(il.Create(OpCodes.Ldloc, aspectVar));
        il.Append(il.Create(OpCodes.Ldloc, contextVar));
        il.Append(il.Create(OpCodes.Ldloc, exceptionVar));
        il.Append(il.Create(OpCodes.Callvirt, onException));
        il.Append(il.Create(OpCodes.Leave, exitPoint));

        // --- E. FINALLY (Handling Sync/Async differently) ---
        il.Append(handlerFinallyStart);

        if (isAsync)
        {
            // For Async, we wrap the returned Task with a continuation
            // Logic: returnVariable.ContinueWith(t => { aspect.OnExit(context); });
            // This is a simplified conceptual flow; usually we use Task.WhenAny or 
            // inject a custom awaiter. For this weaver, we capture the Task object itself.
            InjectAsyncContinuation(il, returnVariable, aspectVar, contextVar, onExit);
        }
        else
        {
            // Standard Sync Return Value Capture
            if (!isVoid)
            {
                il.Append(il.Create(OpCodes.Ldloc, contextVar));
                il.Append(il.Create(OpCodes.Ldloc, returnVariable));
                if (method.ReturnType.IsValueType) il.Append(il.Create(OpCodes.Box, method.ReturnType));
                il.Append(il.Create(OpCodes.Callvirt, setReturnValue));
            }

            il.Append(il.Create(OpCodes.Ldloc, aspectVar));
            il.Append(il.Create(OpCodes.Ldloc, contextVar));
            il.Append(il.Create(OpCodes.Callvirt, onExit));
        }

        il.Append(il.Create(OpCodes.Endfinally));

        // --- F. EXIT ---
        il.Append(exitPoint);
        if (!isVoid) il.Append(il.Create(OpCodes.Ldloc, returnVariable));
        il.Append(il.Create(OpCodes.Ret));

        // --- 8. NOW ADD THE HANDLERS ---
        // This connects the structural markers we injected into valid try/catch/finally metadata
        var exceptionType = module.ImportReference(typeof(Exception));
        AddHandlers(method, nopTryStart, handlerCatchStart, handlerFinallyStart, exitPoint, exceptionType);

        // --- 9. FINALIZE ---
        method.Body.OptimizeMacros();
    }
    
    private static void SetProperty(ILProcessor il, VariableDefinition local, string propName, Instruction loadValue)
    {
        var module = il.Body.Method.Module;
    
        // 1. Use Reflection on the actual Type to find the setter
        // This is more reliable than Cecil's Resolve() for external types
        var propertyInfo = typeof(AspectNetContext).GetProperty(propName);
        if (propertyInfo == null || propertyInfo.SetMethod == null)
            throw new Exception($"Property {propName} or its setter not found on AspectContext");

        // 2. Import the setter method into the current module
        var setterReference = module.ImportReference(propertyInfo.SetMethod);

        // 3. Emit the IL
        il.Append(il.Create(OpCodes.Ldloc, local)); // Load the Context instance
        il.Append(loadValue);                       // Load the value (e.g. Ldstr "MethodName")
        il.Append(il.Create(OpCodes.Callvirt, setterReference));
    }

    private static void InjectAsyncContinuation(
        ILProcessor il, 
        VariableDefinition returnVariable, 
        VariableDefinition aspectVar, 
        VariableDefinition contextVar, 
        MethodReference onExit)
    {
        var module = il.Body.Method.Module;
        var setReturnValue = module.ImportReference(typeof(AspectNetContext).GetProperty("ReturnValue").SetMethod);

        // 1. Assign the Task/ValueTask to context.ReturnValue
        il.Append(il.Create(OpCodes.Ldloc, contextVar));
        il.Append(il.Create(OpCodes.Ldloc, returnVariable));
    
        // Tasks are reference types, but ValueTask is a ValueType and needs boxing
        if (returnVariable.VariableType.IsValueType)
            il.Append(il.Create(OpCodes.Box, returnVariable.VariableType));

        il.Append(il.Create(OpCodes.Callvirt, setReturnValue));

        // 2. Call OnExit immediately
        // Note: In async methods, this fires when the Task is CREATED.
        // To handle completion, your C# OnExit should do: 
        // if (context.ReturnValue is Task t) t.ContinueWith(...)
        il.Append(il.Create(OpCodes.Ldloc, aspectVar));
        il.Append(il.Create(OpCodes.Ldloc, contextVar));
        il.Append(il.Create(OpCodes.Callvirt, onExit));
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