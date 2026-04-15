using System.Collections;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SimpleBitware.AspectNet.Abstractions;
using SimpleBitware.AspectNet.Abstractions.Attributes;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

public static class ILProcessorExtensions
{
    public static Instruction[] AddMethodReturn(this ILProcessor processor, MethodDefinition method)
    {
        var instructions = new List<Instruction>();
        
        // CRITICAL: Load the return value back onto the stack AFTER all try/finally layers
        if (method.ReturnType.MetadataType != MetadataType.Void && !method.IsConstructor)
        {
            // Find the return variable we used in the layers
            var returnVar = method.Body.Variables.FirstOrDefault(v => v.VariableType.FullName == method.ReturnType.FullName);
            if (returnVar != null)
                instructions.Add(processor.Create(OpCodes.Ldloc, returnVar));
        }

        instructions.Add(processor.Create(OpCodes.Ret));
        
        return instructions.ToArray();
    }

    public static Instruction[] CreateAspectContext<T>(
        this ILProcessor processor,
        ModuleDefinition module,
        VariableDefinition entryContextVar,
        MethodDefinition method)
    {
        var instanceSetMethod = module.ImportReference(typeof(T).GetProperty(nameof(AspectNetAttributeContext.Instance))!.SetMethod);
        var getTypeFromHandleMethod = module.ImportReference(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), [typeof(RuntimeTypeHandle)])); //TODO: move out to run it only once
        var classTypeSetMethod = module.ImportReference(typeof(T).GetProperty(nameof(AspectNetAttributeContext.ClassType))!.SetMethod);
        var memberNameSetMethod = module.ImportReference(typeof(T).GetProperty(nameof(AspectNetAttributeContext.MemberName))!.SetMethod);
        var parametersGetMethod = module.ImportReference(typeof(T).GetProperty(nameof(AspectNetAttributeContext.Parameters))!.GetMethod);

        return new List<Instruction>()
            {
                processor.Create(OpCodes.Newobj, module.ImportReference(typeof(T).GetConstructor(Type.EmptyTypes))),
                processor.Create(OpCodes.Stloc, entryContextVar)
            }
            .Concat(processor.SetObjectProperty(
                entryContextVar,
                method.HasThis ? method.Body.ThisParameter : null,
                instanceSetMethod))
            .Concat(processor.SetTypeProperty(
                entryContextVar,
                method.DeclaringType,
                getTypeFromHandleMethod,
                classTypeSetMethod))
            .Concat(processor.SetStringProperty(
                entryContextVar,
                method.Name,
                memberNameSetMethod))
            .Concat(processor.SetDictionaryProperty(
                entryContextVar,
                method.Parameters,
                parametersGetMethod,
                module.ImportReference(typeof(Dictionary<string, object>).GetMethod(nameof(IList.Add), [typeof(string), typeof(object)]))
            ))
            .ToArray();
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

    private static Instruction[] SetStringProperty(
        this ILProcessor processor,
        VariableDefinition entryContextVar,
        string propertyValue,
        MethodReference methodReference)
    {
        return
        [
            processor.Create(OpCodes.Ldloc, entryContextVar),
            processor.Create(OpCodes.Ldstr, propertyValue),
            processor.Create(OpCodes.Callvirt, methodReference)
        ];
    }

    private static Instruction[] SetTypeProperty(
        this ILProcessor processor,
        VariableDefinition entryContextVar,
        TypeReference declaringType,
        MethodReference getTypeFromHandle,
        MethodReference setClassTypeMethod)
    {
        return
        [
            processor.Create(OpCodes.Ldloc, entryContextVar), // Load the context variable (the 'val' local)
            processor.Create(OpCodes.Ldtoken, declaringType), // typeof(DeclaringClass)    
            processor.Create(OpCodes.Call, getTypeFromHandle),
            processor.Create(OpCodes.Callvirt, setClassTypeMethod) // val.ClassType = [result of GetTypeFromHandle]
        ];
    }

    private static Instruction[] SetObjectProperty(
        this ILProcessor processor,
        VariableDefinition entryContextVar,
        ParameterDefinition? propertyValue,
        MethodReference methodReference)
    {
        return
        [
            processor.Create(OpCodes.Ldloc, entryContextVar),
            propertyValue == null
                ? processor.Create(OpCodes.Ldnull) // It's a static method: Load 'null'
                : processor.Create(OpCodes.Ldarg, propertyValue), // It's an instance method: Load 'this'
            processor.Create(OpCodes.Callvirt, methodReference)
        ];
    }

    private static Instruction[] SetDictionaryProperty(
        this ILProcessor processor,
        VariableDefinition entryContextVar,
        IList<ParameterDefinition> parameters,
        MethodReference parametersGetMethod,
        MethodReference addToDictionary)
    {
        var instructions = new List<Instruction>();
        foreach (var param in parameters)
        {
            instructions.AddRange(
            [
                processor.Create(OpCodes.Ldloc, entryContextVar),
                processor.Create(OpCodes.Callvirt, parametersGetMethod), // Push Dictionary
                processor.Create(OpCodes.Ldstr, param.Name), // Push Key
                processor.Create(OpCodes.Ldarg, param), // Push Value
            ]);

            if (param.ParameterType.IsValueType || param.ParameterType is GenericParameter)
                instructions.Add(processor.Create(OpCodes.Box, param.ParameterType));

            instructions.Add(processor.Create(OpCodes.Callvirt, addToDictionary));
        }

        return instructions.ToArray();
    }

    public static Instruction[] SetIntegerProperty(
        this ILProcessor processor,
        ModuleDefinition module,
        VariableDefinition variableDefinition,
        MethodDefinition? propertySetMethod,
        int value)
    {
        return propertySetMethod == null
            ? []
            :
            [
                processor.Create(OpCodes.Ldloc, variableDefinition),
                processor.Create(OpCodes.Ldc_I4, value),
                processor.Create(OpCodes.Callvirt, module.ImportReference(propertySetMethod))
            ];
    }
}
