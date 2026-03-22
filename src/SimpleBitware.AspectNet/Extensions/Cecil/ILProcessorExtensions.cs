using System.Collections;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MoreLinq;
using SimpleBitware.AspectNet.Abstractions;

namespace SimpleBitware.AspectNet.Extensions.Cecil;

public static class ILProcessorExtensions
{
    public static void PushValue(this ILProcessor processor, object value, TypeReference type)
    {
        switch (value)
        {
            case string s:
                processor.Append(processor.Create(OpCodes.Ldstr, s));
                break;
            case int i:
                processor.Append(processor.Create(OpCodes.Ldc_I4, i));
                break;
            case bool b:
                processor.Append(processor.Create(b ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0));
                break;
            case null:
                processor.Append(processor.Create(OpCodes.Ldnull));
                break;
        }
        // Add more types (double, float, etc.) as needed
    }

    public static void CreateEntryContext<T>(
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
        
        method.Body.Variables.Add(entryContextVar);
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
        instructions.ForEach(instruction => processor.Append(instruction));
        return processor;
    }
}
