using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using SimpleBitware.AspectNet.Abstractions;
using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Cecil.Builders;
using SimpleBitware.AspectNet.Cecil.Helpers;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Extensions;

/// <summary>
/// Provides extension methods for weaving aspect-oriented programming logic into method definitions.
/// </summary>
/// <remarks>
/// This class contains the core weaving logic for AspectNet, including method body transformation,
/// try-catch-finally block generation, and aspect attribute processing.
/// </remarks>
public static class MethodDefinitionExtensions
{
    /// <summary>
    /// Applies a marker attribute to a weaved method to indicate it has been processed.
    /// </summary>
    /// <param name="method">The method definition to apply the marker to.</param>
    /// <param name="markerAttributeConstructor">The constructor of the marker attribute to apply.</param>
    /// <remarks>
    /// This method adds a custom attribute to the method to mark it as having been processed
    /// by the aspect weaver, preventing duplicate processing.
    /// </remarks>
    public static void ApplyMarkerAttribute(this MethodDefinition method, MethodReference markerAttributeConstructor)
    {
        method.CustomAttributes.Add(new CustomAttribute(markerAttributeConstructor));
    }

    /// <summary>
    /// Optimizes the macros in a method's body.
    /// </summary>
    /// <param name="method">The method definition to optimize.</param>
    /// <returns>The optimized method definition.</returns>
    /// <remarks>
    /// This method applies Mono.Cecil's macro optimizations to the method body,
    /// which can improve performance and reduce IL size.
    /// </remarks>
    public static MethodDefinition OptimizeMacros(this MethodDefinition method)
    {
        method.Body.OptimizeMacros();
        return method;
    }

    /// <summary>
    /// Weaves a method's body into try-catch-finally blocks for each AspectNet attribute.
    /// </summary>
    /// <param name="methodWithAspects">A key-value pair containing the method and its associated aspect attributes.</param>
    /// <returns>The weaved method definition.</returns>
    /// <remarks>
    /// This is the core weaving method that transforms a method to include aspect-oriented behavior.
    /// It creates nested try-catch-finally blocks for each aspect attribute, ordered by priority,
    /// and handles both synchronous and asynchronous methods.
    /// </remarks>
    public static MethodDefinition WeaveMethod(this KeyValuePair<MethodDefinition, CustomAttribute[]> methodWithAspects)
    {
        var method = methodWithAspects.Key;
        var aspectAttributes = methodWithAspects.Value;
        var methodStartInstructions = method.GetStartInstructions();
        var innerInstructions = method.GetInnerInstructions();
        var isAsyncMethod = method.ReturnType.IsTaskOrValueTaskType();

        var moduleCache = method.Module.Cache();
        var processor = method.Body.GetILProcessor();
        var contextReferences = new AspectContextReferences(moduleCache);
        var aspectReferences = new AspectReferences(moduleCache);

        var contextVariableDefinition = new VariableDefinition(moduleCache.ImportReference(typeof(AspectNetAttributeContext)));
        var returnValueVariableDefinition = method.FindOrCreateReturnVariable();
        var returnTypeReference = moduleCache.ImportReference(method.ReturnType);
        var instructionSet = new InstructionSet()
        {
            Instructions = innerInstructions
        };

        var orderedAspects = aspectAttributes
            .Select((attribute, index) => (CustomAttribute: attribute, Index: index))
            .OrderBy(x => x.CustomAttribute.GetPriorityValue())
            .ThenBy(x => x.Index)
            .Select(x => x.CustomAttribute)
            .Reverse();

        new MethodBodyBuilder(method, processor, moduleCache)
            .ClearMethodBody()
            .AddInstructions(methodStartInstructions)
            .AddVariable(returnValueVariableDefinition)
            .AssignDefaultValueToVariable(returnValueVariableDefinition, method.ReturnType)
            .AddVariable(contextVariableDefinition)
            .AddInstanceVariable<AspectNetAttributeContext>(instanceVariableBuilder => instanceVariableBuilder
                .AssignResultToVariable(contextVariableDefinition)
                .SetStringProperty(contextVariableDefinition, contextReferences.NameSetMethod, method.Name)
                .SetObjectProperty(contextVariableDefinition, contextReferences.InstanceSetMethod, method.HasThis ? method.Body.ThisParameter : null)
                .SetTypeProperty(contextVariableDefinition, typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.ClassType)), method.DeclaringType)
                .SetDictionaryProperty<string, object>(contextVariableDefinition, typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.Parameters)), method.Parameters)
                .Build()
            )
            .ForEachAsOnion(
                orderedAspects,
                instructionSet,
                (builder, customAttribute, currentInstructionSet) =>
                {
                    var isInnermost = (currentInstructionSet == instructionSet);
                    var aspectVariableDefinition = new VariableDefinition(moduleCache.ImportReference(customAttribute.AttributeType));
                    var exceptionVariableDefinition = new VariableDefinition(moduleCache.ImportReference(typeof(Exception)));
                    var builtInstructionSet = builder
                        .AddVariable(aspectVariableDefinition)
                        .AddVariable(exceptionVariableDefinition)
                        .Execute(typeof(AspectNetDependencyInjection).GetMethod(nameof(AspectNetDependencyInjection.GetRequiredService)), aspectVariableDefinition.VariableType)
                        .AddInstanceVariable(instanceVariableBuilder => instanceVariableBuilder
                            .AssignResultToVariable(aspectVariableDefinition)
                            .SetIntProperty(
                                aspectVariableDefinition,
                                moduleCache.ImportReference(customAttribute.AttributeType, MemberNameHelper.PropertySetterName(nameof(IAspectNetAttribute.Priority)), 1),
                                customAttribute.GetPriorityValue())
                            .Build()
                        )
                        .AddTryCatch(
                            tryBlockBuilder => tryBlockBuilder
                                .ExecuteInstanceMethod(aspectVariableDefinition, aspectReferences.OnEntry, contextVariableDefinition)
                                .If(isAsyncMethod,
                                    ifBlockBuilder => ifBlockBuilder
                                        .AddTryBlockForAsyncMethods(
                                            processor,
                                            moduleCache,
                                            returnValueVariableDefinition,
                                            contextVariableDefinition,
                                            aspectVariableDefinition,
                                            returnTypeReference,
                                            isInnermost,
                                            currentInstructionSet)
                                        .Build(),
                                    elseBlockBuilder => elseBlockBuilder
                                        .AddTryBlockForSyncMethods(
                                            returnValueVariableDefinition,
                                            contextVariableDefinition,
                                            aspectVariableDefinition,
                                            aspectReferences,
                                            isInnermost,
                                            currentInstructionSet)
                                        .Build()
                                )
                                .Build(),
                            catchBlockBuilder => catchBlockBuilder
                                .AddInstanceVariable(assignExceptionToContextBlockBuilder =>
                                    assignExceptionToContextBlockBuilder
                                        .AssignResultToVariable(exceptionVariableDefinition)
                                        .SetObjectProperty(contextVariableDefinition,
                                            typeof(AspectNetAttributeContext).GetProperty(nameof(AspectNetAttributeContext.Exception)),
                                            exceptionVariableDefinition)
                                        .Build()
                                )
                                .ExecuteInstanceMethod(aspectVariableDefinition, aspectReferences.OnException, contextVariableDefinition)
                                .ExecuteInstanceMethod(exceptionVariableDefinition, contextReferences.ExceptionGetMethod, contextVariableDefinition)
                                .RethrowWhenEqual()
                                .ExecuteInstanceMethod(contextVariableDefinition, contextReferences.ExceptionGetMethod)
                                .ThrowWhenDifferent(contextVariableDefinition, contextReferences.ExceptionGetMethod)
                                .Build(),
                            finallyBlockBuilder => finallyBlockBuilder
                                .If(isAsyncMethod,
                                    ifBlockBuilder => ifBlockBuilder
                                        .AddFinallyBlockForAsyncMethods(
                                            contextVariableDefinition,
                                            aspectVariableDefinition,
                                            aspectReferences,
                                            contextReferences,
                                            processor)
                                        .Build(),
                                    elseBlockBuilder => elseBlockBuilder
                                        .AddFinallyBlockForSyncMethods(
                                            returnValueVariableDefinition,
                                            contextVariableDefinition,
                                            aspectVariableDefinition,
                                            aspectReferences,
                                            contextReferences,
                                            returnTypeReference)
                                        .Build()
                                )
                                .Build()
                        )
                        .Build();
                    method.RemoveAttribute(customAttribute);
                    return builtInstructionSet;
                })
            .AddReturn(returnValueVariableDefinition, returnTypeReference, isAsyncMethod, method.Module)
            .Build()
            .Apply(processor);

        return method;
    }

    /// <summary>
    /// Removes a custom attribute from a method and its associated property if applicable.
    /// </summary>
    /// <param name="method">The method definition to remove the attribute from.</param>
    /// <param name="attribute">The custom attribute to remove.</param>
    /// <remarks>
    /// This method removes the attribute from the method's custom attributes collection.
    /// If the method is a property accessor, it also removes the attribute from the property itself.
    /// </remarks>
    private static void RemoveAttribute(this MethodDefinition method, CustomAttribute attribute)
    {
        method.CustomAttributes.Remove(attribute);

        var property = method.DeclaringType.Properties
            .FirstOrDefault(p => p.GetMethod == method || p.SetMethod == method);

        property?.CustomAttributes.Remove(attribute);
    }

    /// <summary>
    /// Creates a generic instance of a method reference with the specified type arguments.
    /// </summary>
    /// <param name="method">The method reference to make generic.</param>
    /// <param name="args">The type arguments to use for the generic method.</param>
    /// <returns>A generic instance method reference.</returns>
    /// <remarks>
    /// This method is used to create closed generic method references from open generic method definitions.
    /// </remarks>
    public static MethodReference MakeGeneric(this MethodReference method, params TypeReference[] args)
    {
        var genericType = new GenericInstanceMethod(method);
        foreach (var arg in args) genericType.GenericArguments.Add(arg);
        return genericType;
    }

    /// <summary>
    /// Gets the start instructions of a method, typically the base constructor call for constructors.
    /// </summary>
    /// <param name="method">The method definition to extract start instructions from.</param>
    /// <returns>An array of instructions that should be executed at the start of the method.</returns>
    /// <remarks>
    /// For constructors, this returns the instructions up to and including the base constructor call.
    /// For other methods, returns an empty array.
    /// </remarks>
    private static Instruction[] GetStartInstructions(this MethodDefinition method)
    {
        if (!method.IsConstructor)
            return [];

        var originalInstructions = method.Body.Instructions.ToList();
        var baseCall = originalInstructions.FirstOrDefault(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference { Name: Constants.InstanceConstructorMethodName });
        if (baseCall == null)
            return [];

        var index = originalInstructions.IndexOf(baseCall);
        return originalInstructions.Take(index + 1).ToArray();
    }

    /// <summary>
    /// Gets the inner instructions of a method, excluding the start instructions.
    /// </summary>
    /// <param name="method">The method definition to extract inner instructions from.</param>
    /// <returns>An array of instructions that form the main body of the method.</returns>
    /// <remarks>
    /// For constructors, this returns the instructions after the base constructor call.
    /// For other methods, returns all instructions.
    /// </remarks>
    private static Instruction[] GetInnerInstructions(this MethodDefinition method)
    {
        var originalInstructions = method.Body.Instructions.ToList();
        if (!method.IsConstructor)
            return originalInstructions.ToArray();

        var baseCall = originalInstructions.FirstOrDefault(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference { Name: Constants.InstanceConstructorMethodName });
        if (baseCall == null)
            return originalInstructions.ToArray();

        var index = originalInstructions.IndexOf(baseCall);
        return originalInstructions.Skip(index + 1).ToArray();
    }

    /// <summary>
    /// Finds or creates a return value variable for the method.
    /// </summary>
    /// <param name="method">The method definition to find or create a return variable for.</param>
    /// <returns>The return value variable, or null for void methods or constructors.</returns>
    /// <remarks>
    /// This method looks for an existing variable with the same type as the method's return type.
    /// If none exists and the method is not void, it creates a new variable.
    /// </remarks>
    private static VariableDefinition? FindOrCreateReturnVariable(this MethodDefinition method)
    {
        var isVoid = method.ReturnType.MetadataType == MetadataType.Void || method.IsConstructor;
        return isVoid
            ? null
            : method.Body.Variables.FirstOrDefault(v => v.VariableType.FullName == method.ReturnType.FullName) ?? new VariableDefinition(method.ReturnType);
    }
}
