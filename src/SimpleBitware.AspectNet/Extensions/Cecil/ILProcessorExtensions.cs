using System.Collections;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MoreLinq;
using SimpleBitware.AspectNet.Abstractions;
using SimpleBitware.AspectNet.Abstractions.Context;

namespace SimpleBitware.AspectNet.Extensions.Cecil;

public static class ILProcessorExtensions
{
    public static void AddMethodReturn(this ILProcessor processor, MethodDefinition method)
    {
        // CRITICAL FIX: Load the return value back onto the stack AFTER all try/finally layers
        if (method.ReturnType.MetadataType != MetadataType.Void && !method.IsConstructor)
        {
            // Find the return variable we used in the layers
            var returnVar = method.Body.Variables.FirstOrDefault(v => v.VariableType.FullName == method.ReturnType.FullName);
            if (returnVar != null)
                processor.Emit(OpCodes.Ldloc, returnVar);
        }

        processor.Emit(OpCodes.Ret);
    }

    public static ILProcessor AppendInstructions(this ILProcessor processor, Instruction[] instructions)
    {
        instructions.ForEach(processor.Append);
        return processor;
    }

    public static ILProcessor CreateEntryContext<T>(
        this ILProcessor processor,
        ModuleDefinition module,
        VariableDefinition entryContextVar,
        MethodDefinition method)
    {
        processor.Emit(OpCodes.Newobj, module.ImportReference(typeof(T).GetConstructor(Type.EmptyTypes)));
        processor.Emit(OpCodes.Stloc, entryContextVar);

        processor.SetStringProperty(
            entryContextVar,
            method.DeclaringType.FullName,
            module.GetPropertySetMethodReference<T>(nameof(AbstractAspectNetContext.ClassName)));

        processor.SetStringProperty(
            entryContextVar,
            method.Name,
            module.GetPropertySetMethodReference<T>(nameof(AbstractAspectNetContext.MemberName)));

        processor.SetDictionaryProperty(
            entryContextVar,
            method.Parameters,
            module.GetPropertyGetMethodReference<T>(nameof(AbstractAspectNetContext.Parameters)),
            module.ImportReference(typeof(Dictionary<string, object>).GetMethod(nameof(IList.Add), new[] { typeof(string), typeof(object) }))
        );

        if (!method.Body.Variables.Contains(entryContextVar))
            method.Body.Variables.Add(entryContextVar);

        return processor;
    }

    public static ILProcessor CreateExitContext<T>(
        this ILProcessor processor,
        ModuleDefinition module,
        VariableDefinition exitContextVar,
        VariableDefinition entryContextVar,
        MethodDefinition method)
    {
        // 1. Push arguments onto the stack FIRST
        processor.Emit(OpCodes.Ldloc, entryContextVar);
        processor.Emit(OpCodes.Ldnull);

        // 2. Now call Newobj
        var ctor = typeof(T).GetConstructor([typeof(AspectNetEntryContext), typeof(object)]);
        processor.Emit(OpCodes.Newobj, module.ImportReference(ctor));

        // 3. Store the result
        processor.Emit(OpCodes.Stloc, exitContextVar);

        // Ensure the variable is registered if not already present
        if (!method.Body.Variables.Contains(exitContextVar))
            method.Body.Variables.Add(exitContextVar);

        return processor;
    }

    public static Instruction[] CreateGetAspectInstanceBlock(
        this ILProcessor processor,
        ModuleDefinition module,
        VariableDefinition aspectVar)
    {
        var getService = module.ImportReference(typeof(AspectNetDependencyInjection).GetMethod(nameof(AspectNetDependencyInjection.GetRequiredService)))
            .MakeGeneric(aspectVar.VariableType);

        return
        [
            processor.Create(OpCodes.Call, getService),
            processor.Create(OpCodes.Stloc, aspectVar)
        ];
    }

    public static Instruction[] CreateOnExceptionBlock(
        this ILProcessor processor,
        VariableDefinition entryContextVar,
        VariableDefinition exceptionVariableDefinition,
        MethodReference exceptionContextConstructor,
        VariableDefinition exceptionContext,
        VariableDefinition aspectVariableDefinition,
        MethodReference onException,
        MethodReference getExceptionMethod)
    {
        return [
            // context2 = new AspectNetExceptionContext(val, ex);
            processor.Create(OpCodes.Stloc, exceptionVariableDefinition),
            processor.Create(OpCodes.Ldloc, entryContextVar),
            processor.Create(OpCodes.Ldloc, exceptionVariableDefinition),
            processor.Create(OpCodes.Newobj, exceptionContextConstructor),
            processor.Create(OpCodes.Stloc, exceptionContext),

            // requiredService.OnException(context2);
            processor.Create(OpCodes.Ldloc, aspectVariableDefinition),
            processor.Create(OpCodes.Ldloc, exceptionContext),
            processor.Create(OpCodes.Callvirt, onException),

            // Push ex and context2.Exception for comparison
            processor.Create(OpCodes.Ldloc, exceptionVariableDefinition),
            processor.Create(OpCodes.Ldloc, exceptionContext),
            processor.Create(OpCodes.Callvirt, getExceptionMethod),
        
            // Compare: pops 2 refs, pushes 1 int32 (0 or 1)
            processor.Create(OpCodes.Ceq) 
        ];
    }
    
    public static Instruction[] CloseCatchBlock(
        this ILProcessor processor,
        VariableDefinition originalException,        // The 'ex' caught at the start of catch
        VariableDefinition exceptionContext,        // The AspectNetExceptionContext
        MethodReference getExceptionMethod,         // Context.get_Exception()
        Instruction exitPoint)                      // The 'nop' after the finally block
    {
        var labelCheckNew = processor.Create(OpCodes.Ldloc, exceptionContext);
        var swallowAndLeave = processor.Create(OpCodes.Pop);
    
        return [
            // --- 1. RETHROW LOGIC ---
            // Check if originalException == exceptionContext.Exception
            processor.Create(OpCodes.Ldloc, originalException),
            processor.Create(OpCodes.Ldloc, exceptionContext),
            processor.Create(OpCodes.Callvirt, getExceptionMethod),
            processor.Create(OpCodes.Ceq), 
        
            // If they are equal (1), jump to the 'CheckNew' logic? 
            // No, if equal, we rethrow the original state.
            processor.Create(OpCodes.Brfalse_S, labelCheckNew),
            processor.Create(OpCodes.Rethrow),

            // --- 2. NEW EXCEPTION LOGIC ---
            // If we are here, the aspect modified the exception or we chose not to rethrow
            labelCheckNew,
            processor.Create(OpCodes.Ldloc, exceptionContext),
            processor.Create(OpCodes.Callvirt, getExceptionMethod),
    
            processor.Create(OpCodes.Dup), // Duplicate the exception reference
            processor.Create(OpCodes.Brfalse_S, swallowAndLeave), // If null, go to Pop (swallow)
    
            processor.Create(OpCodes.Throw), // If not null, throw the new exception

            // --- 3. SWALLOW & EXIT ---
            swallowAndLeave,                            // Clean the 'dup' off the stack
            processor.Create(OpCodes.Leave, exitPoint)  // Jump out (triggers finally automatically)
        ];
    }

    public static IEnumerable<Instruction> CreateOnExitBlock(
        this ILProcessor processor,
        VariableDefinition? returnVar,
        bool returnTypeIsValueType,
        VariableDefinition exitContext,
        VariableDefinition aspectVariableDefinition,
        MethodReference exitContextReturnValueGetMethod,
        MethodReference exitContextReturnValueSetMethod,
        TypeReference returnTypeReference,
        MethodReference onExit)
    {
        if (returnVar != null)
        {
            yield return processor.Create(OpCodes.Ldloc, exitContext);
            yield return processor.Create(OpCodes.Ldloc, returnVar);

            if (returnTypeIsValueType)
                yield return processor.Create(OpCodes.Box, returnTypeReference);

            yield return processor.Create(OpCodes.Callvirt, exitContextReturnValueSetMethod);
        }

        // Call OnExit using the SHARED exitContext
        yield return processor.Create(OpCodes.Ldloc, aspectVariableDefinition);
        yield return processor.Create(OpCodes.Ldloc, exitContext);
        yield return processor.Create(OpCodes.Callvirt, onExit);

        // Interceptor Sync: Read possibly modified ReturnValue back from SHARED context
        if (returnVar == null) 
            yield break;
        
        yield return processor.Create(OpCodes.Ldloc, exitContext);
        yield return processor.Create(OpCodes.Callvirt, exitContextReturnValueGetMethod);
        yield return processor.Create(OpCodes.Unbox_Any, returnTypeReference);
        yield return processor.Create(OpCodes.Stloc, returnVar);
    }

    public static IEnumerable<Instruction> CreateMethodInnerInstructionsBlock(
        this ILProcessor processor,
        Instruction[] instructions,
        VariableDefinition? returnVar,
        Instruction exitPoint)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.OpCode == OpCodes.Ret)
            {
                if (returnVar != null)
                    yield return processor.Create(OpCodes.Stloc, returnVar);

                yield return processor.Create(OpCodes.Leave, exitPoint);
            }
            else
            {
                yield return instruction;
            }
        }
    }

    public static Instruction[] CreateOnEntryBlock(
        this ILProcessor processor,
        VariableDefinition aspectVar,
        VariableDefinition entryContext,
        MethodReference onEntry)
    {
        return
        [
            processor.Create(OpCodes.Ldloc, aspectVar),
            processor.Create(OpCodes.Ldloc, entryContext),
            processor.Create(OpCodes.Callvirt, onEntry)
        ];
    }

    private static void SetStringProperty(
        this ILProcessor processor,
        VariableDefinition entryContextVar,
        string propertyValue,
        MethodReference methodReference)
    {
        processor.Emit(OpCodes.Ldloc, entryContextVar);
        processor.Emit(OpCodes.Ldstr, propertyValue);
        processor.Emit(OpCodes.Callvirt, methodReference);
    }

    private static void SetDictionaryProperty(
        this ILProcessor processor,
        VariableDefinition entryContextVar,
        IList<ParameterDefinition> parameters,
        MethodReference getParams,
        MethodReference addToDictionary)
    {
        foreach (var param in parameters)
        {
            processor.Emit(OpCodes.Ldloc, entryContextVar);
            processor.Emit(OpCodes.Callvirt, getParams); // Push Dictionary
            processor.Emit(OpCodes.Ldstr, param.Name); // Push Key
            processor.Emit(OpCodes.Ldarg, param); // Push Value

            if (param.ParameterType.IsValueType || param.ParameterType is GenericParameter)
                processor.Emit(OpCodes.Box, param.ParameterType);

            processor.Emit(OpCodes.Callvirt, addToDictionary);
        }
    }
}
