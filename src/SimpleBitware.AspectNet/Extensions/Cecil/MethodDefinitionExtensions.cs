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

        // 1. Resolve Types
        var returnContextType = module.ImportReference(typeof(AspectNetReturnContext));
        var exceptionContextType = module.ImportReference(typeof(AspectNetExceptionContext));
        var exceptionType = module.ImportReference(typeof(Exception));

        // 2. Determine Method Kind
        bool isAsync = method.CustomAttributes.Any(a => a.AttributeType.Name == "AsyncStateMachineAttribute");
        bool isVoid = method.ReturnType.MetadataType == MetadataType.Void;

        // 3. Setup Locals
        var contextVar = new VariableDefinition(returnContextType);
        method.Body.Variables.Add(contextVar);

        var exceptionVar = new VariableDefinition(exceptionType);
        method.Body.Variables.Add(exceptionVar);

        VariableDefinition? returnVariable = isVoid ? null : new VariableDefinition(method.ReturnType);
        if (returnVariable != null) method.Body.Variables.Add(returnVariable);

        // Markers
        var nopTryStart = Instruction.Create(OpCodes.Nop);
        var handlerCatchStart = Instruction.Create(OpCodes.Stloc, exceptionVar);
        var handlerFinallyStart = Instruction.Create(OpCodes.Nop);
        var exitPoint = Instruction.Create(OpCodes.Nop);

        // --- A. INITIALIZE CONTEXT ---
        il.Append(il.Create(OpCodes.Newobj, module.ImportReference(typeof(AspectNetReturnContext).GetConstructor(Type.EmptyTypes))));
        il.Append(il.Create(OpCodes.Stloc, contextVar));

        SetProperty<AspectNetReturnContext>(il, contextVar, "ClassName", il.Create(OpCodes.Ldstr, method.DeclaringType.FullName));
        SetProperty<AspectNetReturnContext>(il, contextVar, "MemberName", il.Create(OpCodes.Ldstr, method.Name));

        // Initialize Parameters Dictionary
        var dictCtor = module.ImportReference(typeof(Dictionary<string, object>).GetConstructor(Type.EmptyTypes));
        var setParams = module.ImportReference(typeof(AspectNetContext).GetProperty("Parameters").SetMethod);
        il.Append(il.Create(OpCodes.Ldloc, contextVar));
        il.Append(il.Create(OpCodes.Newobj, dictCtor));
        il.Append(il.Create(OpCodes.Callvirt, setParams));

        // --- B. TRY BLOCK START ---
        il.Append(nopTryStart);

        // OnEntry
        il.Append(il.Create(OpCodes.Ldloc, aspectVar));
        il.Append(il.Create(OpCodes.Ldloc, contextVar));
        il.Append(il.Create(OpCodes.Callvirt, onEntry));

        // Original Body
        foreach (var instr in originalInstructions)
        {
            if (instr.OpCode == OpCodes.Ret)
            {
                if (returnVariable != null) il.Append(il.Create(OpCodes.Stloc, returnVariable));
                il.Append(il.Create(OpCodes.Leave, exitPoint));
            }
            else il.Append(instr);
        }

        il.Append(il.Create(OpCodes.Leave, exitPoint));

        // --- C. CATCH BLOCK (Sync Exceptions) ---
        il.Append(handlerCatchStart);

        var exContextVar = new VariableDefinition(exceptionContextType);
        method.Body.Variables.Add(exContextVar);
        il.Append(il.Create(OpCodes.Newobj, module.ImportReference(typeof(AspectNetExceptionContext).GetConstructor(Type.EmptyTypes))));
        il.Append(il.Create(OpCodes.Stloc, exContextVar));

        // Set Exception property and call OnException
        SetProperty<AspectNetExceptionContext>(il, exContextVar, "Exception", il.Create(OpCodes.Ldloc, exceptionVar));
        il.Append(il.Create(OpCodes.Ldloc, aspectVar));
        il.Append(il.Create(OpCodes.Ldloc, exContextVar));
        il.Append(il.Create(OpCodes.Callvirt, onException));
        il.Append(il.Create(OpCodes.Leave, exitPoint));

        // --- D. FINALLY BLOCK ---
        il.Append(handlerFinallyStart);

        if (returnVariable != null)
        {
            // Set ReturnValue
            il.Append(il.Create(OpCodes.Ldloc, contextVar));
            il.Append(il.Create(OpCodes.Ldloc, returnVariable));
            if (method.ReturnType.IsValueType) il.Append(il.Create(OpCodes.Box, method.ReturnType));
            il.Append(il.Create(OpCodes.Callvirt, module.ImportReference(typeof(AspectNetReturnContext).GetProperty("ReturnValue").SetMethod)));

            // ASYNC HANDLING: If it's a Task, hook the continuation
            if (isAsync || method.ReturnType.Name.Contains("Task"))
            {
                var handleAsync = module.ImportReference(typeof(AspectNetRuntime).GetMethod("HandleAsyncExtension"));
                il.Append(il.Create(OpCodes.Ldloc, aspectVar));
                il.Append(il.Create(OpCodes.Ldloc, contextVar));
                il.Append(il.Create(OpCodes.Call, handleAsync));
            }
        }

        // OnExit
        il.Append(il.Create(OpCodes.Ldloc, aspectVar));
        il.Append(il.Create(OpCodes.Ldloc, contextVar));
        il.Append(il.Create(OpCodes.Callvirt, onExit));
        il.Append(il.Create(OpCodes.Endfinally));

        // --- E. RETURN ---
        il.Append(exitPoint);
        if (returnVariable != null) il.Append(il.Create(OpCodes.Ldloc, returnVariable));
        il.Append(il.Create(OpCodes.Ret));

        AddHandlers(method, nopTryStart, handlerCatchStart, handlerFinallyStart, exitPoint, exceptionType);
        method.Body.OptimizeMacros();
    }

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
