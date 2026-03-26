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

    public static ILProcessor CreateAspectContext<T>(
        this ILProcessor processor,
        ModuleDefinition module,
        VariableDefinition entryContextVar,
        MethodDefinition method)
    {
        processor.Emit(OpCodes.Newobj, module.ImportReference(typeof(T).GetConstructor(Type.EmptyTypes)));
        processor.Emit(OpCodes.Stloc, entryContextVar);

        processor.SetObjectProperty(
            entryContextVar,
            method.HasThis ? method.Body.ThisParameter : null,
            module.GetPropertySetMethodReference<T>(nameof(AspectNetAttributeContext.Instance)));

        var getTypeFromHandleMethod = module.ImportReference(
            typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), [typeof(RuntimeTypeHandle)])); //TODO: move out to run it only once
        processor.SetTypeProperty(
            entryContextVar,
            method.DeclaringType,
            getTypeFromHandleMethod,
            module.GetPropertySetMethodReference<T>(nameof(AspectNetAttributeContext.ClassType)));
        
        processor.SetStringProperty(
            entryContextVar,
            method.Name,
            module.GetPropertySetMethodReference<T>(nameof(AspectNetAttributeContext.MemberName)));

        processor.SetDictionaryProperty(
            entryContextVar,
            method.Parameters,
            module.GetPropertyGetMethodReference<T>(nameof(AspectNetAttributeContext.Parameters)),
            module.ImportReference(typeof(Dictionary<string, object>).GetMethod(nameof(IList.Add), [typeof(string), typeof(object)]))
        );

        if (!method.Body.Variables.Contains(entryContextVar))
            method.Body.Variables.Add(entryContextVar);

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
        VariableDefinition entryContextVar, // 'val'
        VariableDefinition exceptionVar, // 'ex'
        MethodReference setExceptionMethod, // Context.set_ReturnValue
        MethodReference getExceptionMethod, // Context.get_Exception
        VariableDefinition aspectVar, // 'requiredService'
        MethodReference onException) // Aspect.OnException
    {
        List<Instruction> instructions = [];

        // 1. catch (Exception ex) { stloc.3 }
        instructions.Add(processor.Create(OpCodes.Stloc, exceptionVar));

        // 2. val.Exception = ex; 
        // Load 'val', then 'ex', then call setter. 
        // After Callvirt, the stack is EMPTY.
        instructions.Add(processor.Create(OpCodes.Ldloc, entryContextVar));
        instructions.Add(processor.Create(OpCodes.Ldloc, exceptionVar));
        instructions.Add(processor.Create(OpCodes.Callvirt, setExceptionMethod));

        // 3. requiredService.OnException(val);
        instructions.Add(processor.Create(OpCodes.Ldloc, aspectVar));
        instructions.Add(processor.Create(OpCodes.Ldloc, entryContextVar));
        instructions.Add(processor.Create(OpCodes.Callvirt, onException));

        // 4. if (ex == val.Exception) { throw; }
        var skipRethrow = processor.Create(OpCodes.Nop);
        instructions.Add(processor.Create(OpCodes.Ldloc, exceptionVar)); // Load ORIGINAL 'ex'
        instructions.Add(processor.Create(OpCodes.Ldloc, entryContextVar));
        instructions.Add(processor.Create(OpCodes.Callvirt, getExceptionMethod));
    
        // Compare the two objects on the stack
        instructions.Add(processor.Create(OpCodes.Bne_Un_S, skipRethrow));
        instructions.Add(processor.Create(OpCodes.Rethrow)); // bare 'throw;'
        instructions.Add(skipRethrow);

        // 5. if (val.Exception != null) { throw val.Exception; }
        var endOfBlock = processor.Create(OpCodes.Nop);
        instructions.Add(processor.Create(OpCodes.Ldloc, entryContextVar));
        instructions.Add(processor.Create(OpCodes.Callvirt, getExceptionMethod));
        instructions.Add(processor.Create(OpCodes.Brfalse_S, endOfBlock));
    
        instructions.Add(processor.Create(OpCodes.Ldloc, entryContextVar));
        instructions.Add(processor.Create(OpCodes.Callvirt, getExceptionMethod));
        instructions.Add(processor.Create(OpCodes.Throw)); // 'throw val.Exception;'
        instructions.Add(endOfBlock);

        return instructions.ToArray();
    }

    public static IEnumerable<Instruction> CreateOnExitBlock(
        this ILProcessor processor,
        VariableDefinition? returnValueVariableDefinition,
        bool returnTypeIsValueType,
        VariableDefinition contextVariableDefinition,
        VariableDefinition aspectVariableDefinition,
        MethodReference exitContextReturnValueGetMethod,
        MethodReference exitContextReturnValueSetMethod,
        TypeReference returnTypeReference,
        MethodReference onExit)
    {
        if (returnValueVariableDefinition != null)
        {
            yield return processor.Create(OpCodes.Ldloc, contextVariableDefinition);
            yield return processor.Create(OpCodes.Ldloc, returnValueVariableDefinition);

            if (returnTypeIsValueType)
                yield return processor.Create(OpCodes.Box, returnTypeReference);

            yield return processor.Create(OpCodes.Callvirt, exitContextReturnValueSetMethod);
        }

        // Call OnExit using the SHARED exitContext
        yield return processor.Create(OpCodes.Ldloc, aspectVariableDefinition);
        yield return processor.Create(OpCodes.Ldloc, contextVariableDefinition);
        yield return processor.Create(OpCodes.Callvirt, onExit);

        // Interceptor Sync: Read possibly modified ReturnValue back from SHARED context
        if (returnValueVariableDefinition == null)
            yield break;

        yield return processor.Create(OpCodes.Ldloc, contextVariableDefinition);
        yield return processor.Create(OpCodes.Callvirt, exitContextReturnValueGetMethod);
        yield return processor.Create(OpCodes.Unbox_Any, returnTypeReference);
        yield return processor.Create(OpCodes.Stloc, returnValueVariableDefinition);
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

    public static Instruction[] CreateOnAspectMethodBlock(
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
    
    private static void SetTypeProperty(
        this ILProcessor processor,
        VariableDefinition entryContextVar,
        TypeReference declaringType,
        MethodReference getTypeFromHandle,
        MethodReference setClassTypeMethod)
    {
        // 1. Load the context variable (the 'val' local)
        processor.Emit(OpCodes.Ldloc, entryContextVar);

        // 2. typeof(DeclaringClass)
        processor.Emit(OpCodes.Ldtoken, declaringType);
        processor.Emit(OpCodes.Call, getTypeFromHandle);

        // 3. val.ClassType = [result of GetTypeFromHandle]
        processor.Emit(OpCodes.Callvirt, setClassTypeMethod);
    }

    private static void SetObjectProperty(
        this ILProcessor processor,
        VariableDefinition entryContextVar,
        ParameterDefinition? propertyValue,
        MethodReference methodReference)
    {
        processor.Emit(OpCodes.Ldloc, entryContextVar);
        
        if (propertyValue != null)
        {
            // It's an instance method: Load 'this'
            processor.Emit(OpCodes.Ldarg, propertyValue);
        }
        else
        {
            // It's a static method: Load 'null'
            processor.Emit(OpCodes.Ldnull);
        }
        
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
