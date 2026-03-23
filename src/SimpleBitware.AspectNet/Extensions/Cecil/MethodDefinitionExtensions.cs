using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MoreLinq;
using SimpleBitware.AspectNet.Abstractions.Context;
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
            .Where(customAttribute => customAttribute.AttributeType.Resolve().InheritsFrom(baseAspectNetAttribute))
            .ToArray();
    }

    /// <summary>
    /// Applies Marker Attribute to weaved method.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="markerAttributeConstructor"></param>
    public static void ApplyMarkerAttribute(this MethodDefinition method, MethodReference markerAttributeConstructor)
    {
        method.CustomAttributes.Add(new CustomAttribute(markerAttributeConstructor));
    }

    /// <summary>
    /// Optimizes method.
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public static MethodDefinition OptimizeMacros(this MethodDefinition method)
    {
        method.Body.OptimizeMacros();
        return method;
    }

    /// <summary>
    /// Weaves method's body into try-catch-finally block for each of the aspect net attributes.
    /// </summary>
    /// <param name="methodWithAspects"></param>
    /// <typeparam name="TEntryContext"></typeparam>
    /// <returns>Weaved method definition.</returns>
    public static MethodDefinition WeaveMethod<TEntryContext, TExitContext>(this KeyValuePair<MethodDefinition, CustomAttribute[]> methodWithAspects)
    {
        var method = methodWithAspects.Key;
        var aspectAttributes = methodWithAspects.Value;
        var processor = method.Body.GetILProcessor();
        var module = method.Module;
        var entryContextVar = new VariableDefinition(module.ImportReference(typeof(TEntryContext)));
        var exitContextVar = new VariableDefinition(module.ImportReference(typeof(TExitContext)));

        var methodStartInstructions = method.GetMethodStartInstructions();
        var methodInstructions = method.GetMethodInstructions();

        method.ClearMethodBody();

        processor.AppendInstructions(methodStartInstructions)
            .CreateEntryContext<TEntryContext>(module, entryContextVar, method)
            .CreateExitContext<TExitContext>(module, exitContextVar, entryContextVar, method);

        aspectAttributes
            .OrderBy(customAttribute => customAttribute.GetPriorityValue())
            .Reverse()
            .ForEach(attribute =>
            {
                methodInstructions = WrapInAttributeLayer(method, attribute, methodInstructions.ToArray(), entryContextVar, exitContextVar);
                method.RemoveAttribute(attribute);
            });

        processor.AppendInstructions(methodInstructions)
            .AddMethodReturn(method);

        return method;
    }

    private static void RemoveAttribute(this MethodDefinition method, CustomAttribute attribute)
    {
        method.CustomAttributes.Remove(attribute);

        var property = method.DeclaringType.Properties
            .FirstOrDefault(p => p.GetMethod == method || p.SetMethod == method);

        property?.CustomAttributes.Remove(attribute);
    }

    public static MethodReference MakeGeneric(this MethodReference method, params TypeReference[] args)
    {
        var genericType = new GenericInstanceMethod(method);
        foreach (var arg in args) genericType.GenericArguments.Add(arg);
        return genericType;
    }

    private static Instruction[] GetMethodInstructions(this MethodDefinition method)
    {
        var originalInstructions = method.Body.Instructions.ToList();
        if (!method.IsConstructor)
            return originalInstructions.ToArray();

        var baseCall = originalInstructions.FirstOrDefault(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference { Name: ".ctor" });
        if (baseCall == null)
            return originalInstructions.ToArray();

        var index = originalInstructions.IndexOf(baseCall);
        return originalInstructions.Skip(index + 1).ToArray();
    }

    private static Instruction[] GetMethodStartInstructions(this MethodDefinition method)
    {
        if (!method.IsConstructor)
            return [];

        var originalInstructions = method.Body.Instructions.ToList();
        var baseCall = originalInstructions.FirstOrDefault(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference { Name: ".ctor" });
        if (baseCall == null)
            return [];

        var index = originalInstructions.IndexOf(baseCall);
        return originalInstructions.Take(index + 1).ToArray();
    }

    private static void ClearMethodBody(this MethodDefinition method)
    {
        method.Body.Instructions.Clear();
        method.Body.ExceptionHandlers.Clear();
    }

    private static VariableDefinition? FindOrCreateReturnVariable(this MethodDefinition method)
    {
        var isVoid = method.ReturnType.MetadataType == MetadataType.Void || method.IsConstructor;
        var returnVar = isVoid ? null : method.Body.Variables.FirstOrDefault(v => v.VariableType.FullName == method.ReturnType.FullName);
        if (!isVoid && returnVar == null)
        {
            returnVar = new VariableDefinition(method.ReturnType);
            method.Body.Variables.Add(returnVar);
        }

        return returnVar;
    }

    private static Instruction[] WrapInAttributeLayer(
        MethodDefinition method,
        CustomAttribute customAttribute,
        Instruction[] innerInstructions,
        VariableDefinition entryContext,
        VariableDefinition exitContext)
    {
        var processor = method.Body.GetILProcessor();
        var module = method.Module;
        var aspectReferences = new AspectReferences(module, customAttribute.AttributeType.Resolve());
        var exceptionContextConstructor = module.ImportReference(typeof(AspectNetExceptionContext).GetConstructor(new[] { typeof(AspectNetEntryContext), typeof(Exception) }));
        var exitContextReturnValueGetMethod = module.ImportReference(typeof(AspectNetExitContext).GetProperty(nameof(AspectNetExitContext.ReturnValue))!.GetMethod);
        var exitContextReturnValueSetMethod = module.ImportReference(typeof(AspectNetExitContext).GetProperty(nameof(AspectNetExitContext.ReturnValue))!.SetMethod);
        var returnTypeReference = module.ImportReference(method.ReturnType);

        // Layer Locals
        var aspectVariableDefinition = new VariableDefinition(module.ImportReference(customAttribute.AttributeType));
        var exceptionVariableDefinition = new VariableDefinition(module.ImportReference(typeof(Exception)));
        var exceptionContext = new VariableDefinition(module.ImportReference(typeof(AspectNetExceptionContext)));
        method.Body.Variables.Add(aspectVariableDefinition);
        method.Body.Variables.Add(exceptionVariableDefinition);
        method.Body.Variables.Add(exceptionContext);

        // Find or Create Return Variable for this method
        var returnVar = method.FindOrCreateReturnVariable();

        // Jump Targets
        var handlerTryStart = processor.Create(OpCodes.Nop);
        var handlerCatchStart = processor.Create(OpCodes.Nop);
        var handlerFinallyStart = processor.Create(OpCodes.Nop);
        var exitPoint = processor.Create(OpCodes.Nop);
        
        // Register Exception Handlers
        method.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            TryStart = handlerTryStart,
            TryEnd = handlerCatchStart,
            HandlerStart = handlerCatchStart,
            HandlerEnd = handlerFinallyStart,
            CatchType = module.ImportReference(typeof(Exception))
        });

        method.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Finally)
        {
            TryStart = handlerTryStart,
            TryEnd = handlerFinallyStart,
            HandlerStart = handlerFinallyStart,
            HandlerEnd = exitPoint
        });

        return
            // local variables
            processor.CreateGetAspectInstanceBlock(module, aspectVariableDefinition)
            // try
            .Append(handlerTryStart)
            .Concat(processor.CreateOnEntryBlock(aspectVariableDefinition, entryContext, aspectReferences.OnEntry))
            .Concat(processor.CreateMethodInnerInstructionsBlock(innerInstructions, returnVar, exitPoint))
            .Concat(processor.CloseTryBlock(exitPoint))
            // catch
            .Concat(processor.StartCatchBlock(handlerCatchStart, exceptionVariableDefinition))
            .Concat(processor.CreateOnExceptionBlock(entryContext, exceptionVariableDefinition, exceptionContextConstructor, exceptionContext, aspectVariableDefinition, aspectReferences.OnException))
            .Concat(processor.CloseCatchBlock())
            // finally
            .Append(handlerFinallyStart)
            .Concat(processor.CreateOnExitBlock(returnVar, method.ReturnType.IsValueType, exitContext, aspectVariableDefinition, exitContextReturnValueGetMethod, exitContextReturnValueSetMethod, returnTypeReference, aspectReferences.OnExit))
            .Append(processor.Create(OpCodes.Endfinally))
            // leave method
            .Append(exitPoint)
            .ToArray();
    }
}
