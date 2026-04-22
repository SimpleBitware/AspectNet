using System.Collections;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Builders;

public class InstanceVariableBuilder(MethodDefinition method, ILProcessor processor, ModuleCache moduleCache): InstructionSetBuilderBase<InstanceVariableBuilder>(method, processor, moduleCache)
{
    public InstanceVariableBuilder Create<T>()
    {
        var instruction = Processor.Create(OpCodes.Newobj, ModuleCache.ImportReference(typeof(T).GetConstructor(Type.EmptyTypes)));
        Instructions.Add(instruction);
        return this;
    }
    
    public InstanceVariableBuilder AssignResultToVariable(VariableDefinition? variableDefinition)
    {
        if (variableDefinition is not null)
            Instructions.Add(Processor.Create(OpCodes.Stloc, variableDefinition));

        return this;
    }
    
    public InstanceVariableBuilder SetStringProperty(
        VariableDefinition? variableDefinition,
        MethodReference? methodReference,
        string? propertyValue)
    {
        if (variableDefinition is not null && methodReference is not null)
        {
            Instructions.AddRange([
                Processor.Create(OpCodes.Ldloc, variableDefinition),
                propertyValue is null
                    ? Processor.Create(OpCodes.Ldnull)
                    : Processor.Create(OpCodes.Ldstr, propertyValue),
                Processor.Create(OpCodes.Callvirt, methodReference)
            ]);
        }

        return this;
    }

    public InstanceVariableBuilder SetIntProperty(
        VariableDefinition? variableDefinition,
        MethodReference setMethodReference,
        int? propertyValue)
    {
        if (variableDefinition is not null)
        {
            Instructions.AddRange([
                Processor.Create(OpCodes.Ldloc, variableDefinition),
                propertyValue is null
                    ? Processor.Create(OpCodes.Ldnull)
                    : Processor.Create(OpCodes.Ldc_I4, propertyValue.Value),
                Processor.Create(OpCodes.Callvirt, setMethodReference)
            ]);
        }

        return this;
    }

    public InstanceVariableBuilder SetObjectProperty(
        VariableDefinition? variableDefinition,
        MethodReference? methodReference,
        ParameterDefinition? propertyValue)
    {
        if (variableDefinition is not null && methodReference is not null)
        {
            Instructions.AddRange([
                Processor.Create(OpCodes.Ldloc, variableDefinition),
                propertyValue == null
                    ? Processor.Create(OpCodes.Ldnull)
                    : Processor.Create(OpCodes.Ldarg, propertyValue),
                Processor.Create(OpCodes.Callvirt, methodReference)
            ]);
        }

        return this;
    }

    public InstanceVariableBuilder SetObjectProperty(
        VariableDefinition? variableDefinition,
        PropertyInfo? propertyInfo,
        VariableDefinition? valueVariable)
    {
        if (variableDefinition is not null && propertyInfo is not null)
        {
            var setMethodReference = ModuleCache.ImportReference(propertyInfo.SetMethod);
            Instructions.Add(Processor.Create(OpCodes.Ldloc, variableDefinition));

            if (valueVariable == null)
            {
                Instructions.Add(Processor.Create(OpCodes.Ldnull));
            }
            else
            {
                Instructions.Add(Processor.Create(OpCodes.Ldloc, valueVariable));
                if (valueVariable.VariableType.IsValueType)
                    Instructions.Add(Processor.Create(OpCodes.Box, valueVariable.VariableType));
            }

            Instructions.Add(Processor.Create(OpCodes.Callvirt, setMethodReference));
        }

        return this;
    }

    public InstanceVariableBuilder SetTypeProperty(
        VariableDefinition? variableDefinition,
        PropertyInfo? propertyInfo,
        TypeReference declaringType)
    {
        if (variableDefinition is not null && propertyInfo is not null)
        {
            var getTypeFromHandleMethod = ModuleCache.ImportReference(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), [typeof(RuntimeTypeHandle)]));
            var setMethodReference = ModuleCache.ImportReference(propertyInfo.SetMethod);
            Instructions.AddRange([
                Processor.Create(OpCodes.Ldloc, variableDefinition),
                Processor.Create(OpCodes.Ldtoken, declaringType), // typeof(DeclaringClass)
                Processor.Create(OpCodes.Call, getTypeFromHandleMethod),
                Processor.Create(OpCodes.Callvirt, setMethodReference)
            ]);
        }

        return this;
    }

    public InstanceVariableBuilder SetDictionaryProperty<TKey, TValue>(
        VariableDefinition? variableDefinition,
        PropertyInfo? propertyInfo,
        IList<ParameterDefinition> parameters)
    {
        if (variableDefinition is not null && propertyInfo is not null)
        {
            var getMethodReference = ModuleCache.ImportReference(propertyInfo.GetMethod);
            var addToDictionary = ModuleCache.ImportReference(typeof(Dictionary<TKey, TValue>).GetMethod(nameof(IList.Add), [typeof(TKey), typeof(TValue)]));
            foreach (var param in parameters)
            {
                Instructions.AddRange(
                [
                    Processor.Create(OpCodes.Ldloc, variableDefinition),
                    Processor.Create(OpCodes.Callvirt, getMethodReference), // Push Dictionary
                    Processor.Create(OpCodes.Ldstr, param.Name), // Push Key                            //TODO: use TKey
                    Processor.Create(OpCodes.Ldarg, param), // Push Value                               //TODO: use TValue
                ]);

                if (param.ParameterType.IsValueType || param.ParameterType is GenericParameter)
                    Instructions.Add(Processor.Create(OpCodes.Box, param.ParameterType));

                Instructions.Add(Processor.Create(OpCodes.Callvirt, addToDictionary));
            }
        }

        return this;
    }
}
