using System.Collections;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Builders;

/// <summary>
/// Builds IL instructions for creating and initializing instances of a specified type.
/// </summary>
/// <remarks>
/// This builder specializes in generating IL for object instantiation and property/field initialization.
/// It provides a fluent API for constructing complex initialization sequences including string, int,
/// object, and dictionary property assignments.
/// </remarks>
public class InstanceVariableBuilder(MethodDefinition method, ILProcessor processor, ModuleCache moduleCache): InstructionSetBuilderBase<InstanceVariableBuilder>(method, processor, moduleCache)
{
    /// <summary>
    /// Creates a new instance of type T using its parameterless constructor.
    /// </summary>
    /// <typeparam name="T">The type to instantiate.</typeparam>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This method generates a <c>newobj</c> IL instruction for the specified type's default constructor.
    /// </remarks>
    public InstanceVariableBuilder Create<T>()
    {
        var instruction = Processor.Create(OpCodes.Newobj, ModuleCache.ImportReference(typeof(T).GetConstructor(Type.EmptyTypes)));
        Instructions.Add(instruction);
        return this;
    }
    
    /// <summary>
    /// Assigns the result on the stack to the specified variable.
    /// </summary>
    /// <param name="variableDefinition">The variable to assign the result to, or null to skip assignment.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// If <paramref name="variableDefinition"/> is null, this method has no effect.
    /// Generates a <c>stloc</c> IL instruction.
    /// </remarks>
    public InstanceVariableBuilder AssignResultToVariable(VariableDefinition? variableDefinition)
    {
        if (variableDefinition is not null)
            Instructions.Add(Processor.Create(OpCodes.Stloc, variableDefinition));

        return this;
    }
    
    /// <summary>
    /// Sets a string property on the instance.
    /// </summary>
    /// <param name="variableDefinition">The instance variable, or null to skip operation.</param>
    /// <param name="methodReference">The property setter method, or null to skip operation.</param>
    /// <param name="propertyValue">The string value to set, or null to set the property to null.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// If either <paramref name="variableDefinition"/> or <paramref name="methodReference"/> is null, this method has no effect.
    /// Generates IL that loads the instance, loads the string value (or null), and calls the setter method.
    /// </remarks>
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

    /// <summary>
    /// Sets an integer property on the instance.
    /// </summary>
    /// <param name="variableDefinition">The instance variable, or null to skip operation.</param>
    /// <param name="setMethodReference">The property setter method.</param>
    /// <param name="propertyValue">The integer value to set, or null to set the property to null.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// If <paramref name="variableDefinition"/> is null, this method has no effect.
    /// Generates IL that loads the instance, loads the integer value, and calls the setter method.
    /// </remarks>
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

    /// <summary>
    /// Sets an object property on the instance using a method reference and parameter.
    /// </summary>
    /// <param name="variableDefinition">The instance variable, or null to skip operation.</param>
    /// <param name="methodReference">The property setter method, or null to skip operation.</param>
    /// <param name="propertyValue">The parameter to use as the property value, or null to pass null.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// If either <paramref name="variableDefinition"/> or <paramref name="methodReference"/> is null, this method has no effect.
    /// Generates IL that loads the instance, loads the parameter argument (or null), and calls the setter method.
    /// </remarks>
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

    /// <summary>
    /// Sets an object property on the instance using reflection metadata and a variable.
    /// </summary>
    /// <param name="variableDefinition">The instance variable, or null to skip operation.</param>
    /// <param name="propertyInfo">The property metadata, or null to skip operation.</param>
    /// <param name="valueVariable">The variable containing the value to set, or null to pass null.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// If either <paramref name="variableDefinition"/> or <paramref name="propertyInfo"/> is null, this method has no effect.
    /// Value types are automatically boxed before being passed to the property setter.
    /// </remarks>
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

    /// <summary>
    /// Sets a Type property on the instance to the declaring type.
    /// </summary>
    /// <param name="variableDefinition">The instance variable, or null to skip operation.</param>
    /// <param name="propertyInfo">The property metadata, or null to skip operation.</param>
    /// <param name="declaringType">The type to assign to the property.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// If either <paramref name="variableDefinition"/> or <paramref name="propertyInfo"/> is null, this method has no effect.
    /// Uses <c>ldtoken</c> and <see cref="Type.GetTypeFromHandle"/> to obtain the runtime type.
    /// </remarks>
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

    /// <summary>
    /// Populates a dictionary property with entries from method parameters.
    /// </summary>
    /// <typeparam name="TKey">The key type of the dictionary.</typeparam>
    /// <typeparam name="TValue">The value type of the dictionary.</typeparam>
    /// <param name="variableDefinition">The instance variable, or null to skip operation.</param>
    /// <param name="propertyInfo">The dictionary property metadata, or null to skip operation.</param>
    /// <param name="parameters">The parameters to add to the dictionary.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// If either <paramref name="variableDefinition"/> or <paramref name="propertyInfo"/> is null, this method has no effect.
    /// Each parameter is added to the dictionary with its name as the key and its value as the dictionary value.
    /// Value types are automatically boxed.
    /// </remarks>
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
