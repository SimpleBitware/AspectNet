using System.Collections;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SimpleBitware.AspectNet.Cecil.Extensions;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Builders;

public class MethodBodyBuilder(MethodDefinition method, ILProcessor processor, ModuleCache moduleCache) : InstructionSetBlockBuilderBase<MethodBodyBuilder>(method, processor, moduleCache)
{
    public MethodBodyBuilder ClearMethodBody()
    {
        Method.Body.Instructions.Clear();
        Method.Body.ExceptionHandlers.Clear();
        return this;
    }
    
    // private readonly ModuleCache moduleCache = method.Module.Cache();
    // private readonly ILProcessor processor = method.Body.GetILProcessor();
    //
    // private readonly List<Instruction> instructions = [];
    // private readonly List<ExceptionHandler> exceptionHandlers = [];
    //
    // private ExceptionHandler? currentCatchHandler;
    // private ExceptionHandler? currentFinallyHandler;

    // public MethodBodyBuilder DeclareGenericVariable(VariableDefinition? variableDefinition, TypeReference typeReference)
    // {
    //     if (variableDefinition == null) return this;
    //
    //     method.Body.Variables.Add(variableDefinition);
    //     Instructions.Add(processor.Create(OpCodes.Ldloca, variableDefinition));
    //     Instructions.Add(processor.Create(OpCodes.Initobj, typeReference));
    //     return this;
    // }
    //
    // public MethodBodyBuilder DeclareVariable(VariableDefinition? variableDefinition)
    // {
    //     if (variableDefinition is not null)
    //         method.Body.Variables.Add(variableDefinition);
    //
    //     return this;
    // }

    // public MethodBodyBuilder CreateInstance<T>()
    // {
    //     var instruction = processor.Create(OpCodes.Newobj, moduleCache.ImportReference(typeof(T).GetConstructor(Type.EmptyTypes)));
    //     instructions.Add(instruction);
    //     return this;
    // }
    //
    // public MethodBodyBuilder SetVariable(VariableDefinition? variableDefinition)
    // {
    //     if (variableDefinition is not null)
    //         instructions.Add(processor.Create(OpCodes.Stloc, variableDefinition));
    //
    //     return this;
    // }
    //
    // public MethodBodyBuilder GetValue(
    //     VariableDefinition parameter,
    //     MethodReference? methodReference)
    // {
    //     if (methodReference is not null)
    //     {
    //         instructions.AddRange([
    //             processor.Create(OpCodes.Ldloc, parameter),
    //             processor.Create(OpCodes.Callvirt, methodReference)
    //         ]);
    //     }
    //
    //     return this;
    // }
    //
    // public MethodBodyBuilder SetStringProperty(
    //     VariableDefinition? variableDefinition,
    //     MethodReference? methodReference,
    //     string? propertyValue)
    // {
    //     if (variableDefinition is not null && methodReference is not null)
    //     {
    //         instructions.AddRange([
    //             processor.Create(OpCodes.Ldloc, variableDefinition),
    //             propertyValue is null
    //                 ? processor.Create(OpCodes.Ldnull)
    //                 : processor.Create(OpCodes.Ldstr, propertyValue),
    //             processor.Create(OpCodes.Callvirt, methodReference)
    //         ]);
    //     }
    //
    //     return this;
    // }
    //
    // public MethodBodyBuilder SetIntProperty(
    //     VariableDefinition? variableDefinition,
    //     MethodReference setMethodReference,
    //     int? propertyValue)
    // {
    //     if (variableDefinition is not null)
    //     {
    //         instructions.AddRange([
    //             processor.Create(OpCodes.Ldloc, variableDefinition),
    //             propertyValue is null
    //                 ? processor.Create(OpCodes.Ldnull)
    //                 : processor.Create(OpCodes.Ldc_I4, propertyValue.Value),
    //             processor.Create(OpCodes.Callvirt, setMethodReference)
    //         ]);
    //     }
    //
    //     return this;
    // }
    //
    // public MethodBodyBuilder SetObjectProperty(
    //     VariableDefinition? variableDefinition,
    //     MethodReference? methodReference,
    //     ParameterDefinition? propertyValue)
    // {
    //     if (variableDefinition is not null && methodReference is not null)
    //     {
    //         instructions.AddRange([
    //             processor.Create(OpCodes.Ldloc, variableDefinition),
    //             propertyValue == null
    //                 ? processor.Create(OpCodes.Ldnull)
    //                 : processor.Create(OpCodes.Ldarg, propertyValue),
    //             processor.Create(OpCodes.Callvirt, methodReference)
    //         ]);
    //     }
    //
    //     return this;
    // }
    //
    // public MethodBodyBuilder SetObjectProperty(
    //     VariableDefinition? variableDefinition,
    //     PropertyInfo? propertyInfo,
    //     VariableDefinition? valueVariable)
    // {
    //     if (variableDefinition is not null && propertyInfo is not null)
    //     {
    //         var setMethodReference = moduleCache.ImportReference(propertyInfo.SetMethod);
    //         instructions.Add(processor.Create(OpCodes.Ldloc, variableDefinition));
    //
    //         if (valueVariable == null)
    //         {
    //             instructions.Add(processor.Create(OpCodes.Ldnull));
    //         }
    //         else
    //         {
    //             instructions.Add(processor.Create(OpCodes.Ldloc, valueVariable));
    //             if (valueVariable.VariableType.IsValueType)
    //                 instructions.Add(processor.Create(OpCodes.Box, valueVariable.VariableType));
    //         }
    //
    //         instructions.Add(processor.Create(OpCodes.Callvirt, setMethodReference));
    //     }
    //
    //     return this;
    // }
    //
    // public MethodBodyBuilder SetTypeProperty(
    //     VariableDefinition? variableDefinition,
    //     PropertyInfo? propertyInfo,
    //     TypeReference declaringType)
    // {
    //     if (variableDefinition is not null && propertyInfo is not null)
    //     {
    //         var getTypeFromHandleMethod = moduleCache.ImportReference(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), [typeof(RuntimeTypeHandle)]));
    //         var setMethodReference = moduleCache.ImportReference(propertyInfo.SetMethod);
    //         instructions.AddRange([
    //             processor.Create(OpCodes.Ldloc, variableDefinition),
    //             processor.Create(OpCodes.Ldtoken, declaringType), // typeof(DeclaringClass)    
    //             processor.Create(OpCodes.Call, getTypeFromHandleMethod),
    //             processor.Create(OpCodes.Callvirt, setMethodReference)
    //         ]);
    //     }
    //
    //     return this;
    // }
    //
    // public MethodBodyBuilder SetDictionaryProperty<TKey, TValue>(
    //     VariableDefinition? variableDefinition,
    //     PropertyInfo? propertyInfo,
    //     IList<ParameterDefinition> parameters)
    // {
    //     if (variableDefinition is not null && propertyInfo is not null)
    //     {
    //         var getMethodReference = moduleCache.ImportReference(propertyInfo.GetMethod);
    //         var addToDictionary = moduleCache.ImportReference(typeof(Dictionary<TKey, TValue>).GetMethod(nameof(IList.Add), [typeof(TKey), typeof(TValue)]));
    //         foreach (var param in parameters)
    //         {
    //             instructions.AddRange(
    //             [
    //                 processor.Create(OpCodes.Ldloc, variableDefinition),
    //                 processor.Create(OpCodes.Callvirt, getMethodReference), // Push Dictionary
    //                 processor.Create(OpCodes.Ldstr, param.Name), // Push Key                            //TODO: use TKey
    //                 processor.Create(OpCodes.Ldarg, param), // Push Value                               //TODO: use TValue
    //             ]);
    //
    //             if (param.ParameterType.IsValueType || param.ParameterType is GenericParameter)
    //                 instructions.Add(processor.Create(OpCodes.Box, param.ParameterType));
    //
    //             instructions.Add(processor.Create(OpCodes.Callvirt, addToDictionary));
    //         }
    //     }
    //
    //     return this;
    // }
    //
    // public MethodBodyBuilder SetPropertyOrDefault(
    //     VariableDefinition? variableDefinition,
    //     VariableDefinition instance,
    //     MethodReference? getMethod,
    //     TypeReference returnTypeReference)
    // {
    //     if (variableDefinition is not null && getMethod is not null)
    //     {
    //         // Load val.ReturnValue onto stack
    //         instructions.AddRange([
    //             processor.Create(OpCodes.Ldloc, instance),
    //             processor.Create(OpCodes.Callvirt, getMethod),
    //             processor.Create(OpCodes.Dup)
    //         ]);
    //
    //         // Prepare our jump targets
    //         var unboxTarget = processor.Create(OpCodes.Unbox_Any, returnTypeReference);
    //         var finalStore = processor.Create(OpCodes.Stloc, variableDefinition);
    //
    //         // Null Check
    //         instructions.Add(processor.Create(OpCodes.Brtrue_S, unboxTarget));
    //
    //         // Pop the duped null
    //         instructions.Add(processor.Create(OpCodes.Pop));
    //         if (returnTypeReference.IsValueType || returnTypeReference.IsGenericParameter)
    //         {
    //             // We need default(T) on the stack.
    //             // We use the variable as a temporary buffer to create it.
    //             instructions.AddRange([
    //                 processor.Create(OpCodes.Ldloca, variableDefinition),
    //                 processor.Create(OpCodes.Initobj, returnTypeReference),
    //                 processor.Create(OpCodes.Ldloc, variableDefinition)
    //             ]);
    //         }
    //         else
    //         {
    //             instructions.Add(processor.Create(OpCodes.Ldnull));
    //         }
    //
    //         // Jump to the ONLY store
    //         instructions.Add(processor.Create(OpCodes.Br_S, finalStore));
    //
    //         // --- NOT NULL PATH ---
    //         // Stack now has the unboxed value
    //         instructions.Add(unboxTarget);
    //
    //         // StackValue = (Condition) ? PathA_Stack : PathB_Stack;
    //         // Followed by: num = StackValue;
    //         instructions.Add(finalStore);
    //     }
    //
    //     return this;
    // }
    //
    // public MethodBodyBuilder Execute(
    //     MethodInfo? methodInfo,
    //     TypeReference returnType)
    // {
    //     if (methodInfo is null) return this;
    //
    //     var methodReference = moduleCache.ImportReference(methodInfo)?.MakeGeneric(returnType);
    //     if (methodReference is not null)
    //         instructions.Add(processor.Create(OpCodes.Call, methodReference));
    //
    //     return this;
    // }
    //
    // public MethodBodyBuilder Execute(
    //     VariableDefinition variable,
    //     VariableDefinition parameter,
    //     MethodReference? methodReference)
    // {
    //     if (methodReference is not null)
    //     {
    //         instructions.Add(processor.Create(OpCodes.Ldloc, variable));
    //         GetValue(parameter, methodReference);
    //     }
    //
    //     return this;
    // }
    //
    // public MethodBodyBuilder ExecuteIf(Func<bool> condition, Action<MethodBodyBuilder> action)
    // {
    //     if (condition())
    //     {
    //         action(this);
    //     }
    //
    //     return this;
    // }
    //
    // // Overload for a simple boolean value instead of a Func
    // public MethodBodyBuilder ExecuteIf(bool condition, Action<MethodBodyBuilder> action)
    // {
    //     if (condition)
    //     {
    //         action(this);
    //     }
    //
    //     return this;
    // }
    //
    // public MethodBodyBuilder RethrowWhenEqual()
    // {
    //     var skipToInstruction = processor.Create(OpCodes.Nop);
    //     instructions.AddRange([
    //         processor.Create(OpCodes.Bne_Un_S, skipToInstruction),
    //         processor.Create(OpCodes.Rethrow),
    //         skipToInstruction
    //     ]);
    //
    //     return this;
    // }
    //
    // public MethodBodyBuilder ThrowWhenDifferent(
    //     VariableDefinition variable,
    //     MethodReference? methodReference)
    // {
    //     var skipToInstruction = processor.Create(OpCodes.Nop);
    //     instructions.Add(processor.Create(OpCodes.Brfalse_S, skipToInstruction));
    //     GetValue(variable, methodReference);
    //     instructions.AddRange([
    //         processor.Create(OpCodes.Throw),
    //         skipToInstruction
    //     ]);
    //
    //     return this;
    // }
    //
    // public MethodBodyBuilder DeclareTryCatchFinally()
    // {
    //     var tryStartInstruction = processor.Create(OpCodes.Nop);
    //     var catchStartInstruction = processor.Create(OpCodes.Nop);
    //     var finallyStartInstruction = processor.Create(OpCodes.Nop);
    //     var exitInstruction = processor.Create(OpCodes.Nop);
    //
    //     var catchHandler = new ExceptionHandler(ExceptionHandlerType.Catch)
    //     {
    //         TryStart = tryStartInstruction,
    //         TryEnd = catchStartInstruction,
    //         HandlerStart = catchStartInstruction,
    //         HandlerEnd = finallyStartInstruction,
    //         CatchType = moduleCache.ImportReference(typeof(Exception))
    //     };
    //     var finallyHandler = new ExceptionHandler(ExceptionHandlerType.Finally)
    //     {
    //         TryStart = tryStartInstruction,
    //         TryEnd = finallyStartInstruction,
    //         HandlerStart = finallyStartInstruction,
    //         HandlerEnd = exitInstruction
    //     };
    //
    //     exceptionHandlers.Add(catchHandler);
    //     exceptionHandlers.Add(finallyHandler);
    //
    //     currentCatchHandler = catchHandler;
    //     currentFinallyHandler = finallyHandler;
    //
    //     return this;
    // }
    //
    // public MethodBodyBuilder StartTry()
    // {
    //     var startTryInstruction = currentCatchHandler?.TryStart;
    //     if (startTryInstruction is null)
    //         throw new InvalidOperationException("Try block cannot be started. Ensure 'DeclareTryCatchFinally()' was called first.");
    //
    //     instructions.Add(startTryInstruction);
    //     return this;
    // }
    //
    // public MethodBodyBuilder EndTry()
    // {
    //     var exitPoint = currentFinallyHandler?.HandlerEnd;
    //     if (exitPoint is null)
    //         throw new InvalidOperationException("Cannot end Try block: Boundary 'HandlerEnd' is missing. Did you initialize the handler correctly?");
    //
    //     instructions.Add(processor.Create(OpCodes.Leave, exitPoint));
    //     return this;
    // }
    //
    // public MethodBodyBuilder StartCatch()
    // {
    //     var startCatchInstruction = currentCatchHandler?.HandlerStart;
    //     if (startCatchInstruction is null)
    //         throw new InvalidOperationException("Catch block cannot be started. Ensure 'DeclareTryCatchFinally()' was called first.");
    //
    //     instructions.Add(startCatchInstruction);
    //     return this;
    // }
    //
    // public MethodBodyBuilder EndCatch()
    // {
    //     var exitPoint = currentFinallyHandler?.HandlerEnd;
    //     if (exitPoint is null)
    //         throw new InvalidOperationException("Cannot end Catch block: Boundary 'HandlerEnd' is missing. Ensure the ExceptionHandler state is valid.");
    //
    //     instructions.Add(processor.Create(OpCodes.Leave, exitPoint));
    //     return this;
    // }
    //
    // public MethodBodyBuilder StartFinally()
    // {
    //     var startFinallyInstruction = currentFinallyHandler?.HandlerStart;
    //     if (startFinallyInstruction is null)
    //         throw new InvalidOperationException("Finally block cannot be started. Ensure 'DeclareTryCatchFinally()' was called first.");
    //
    //     instructions.Add(startFinallyInstruction);
    //     return this;
    // }
    //
    // public MethodBodyBuilder EndFinally()
    // {
    //     var exitPoint = currentFinallyHandler?.HandlerEnd;
    //     if (exitPoint is null)
    //         throw new InvalidOperationException("Cannot end a finally block that was never started. Ensure StartFinally() was called.");
    //
    //     instructions.Add(processor.Create(OpCodes.Endfinally));
    //     instructions.Add(exitPoint);
    //     return this;
    // }
    //
    // public MethodBodyBuilder AddInstructions(IEnumerable<Instruction> instructionItems)
    // {
    //     instructions.AddRange(instructionItems);
    //     return this;
    // }
    //
    // public MethodBodyBuilder AddExceptionHandlers(IEnumerable<ExceptionHandler> exceptionHandlerItems)
    // {
    //     exceptionHandlers.AddRange(exceptionHandlerItems);
    //     return this;
    // }
    
    public MethodBodyBuilder AddReturn(VariableDefinition? variableDefinition)
    {
        if (variableDefinition is not null)
            Instructions.Add(Processor.Create(OpCodes.Ldloc, variableDefinition));

        Instructions.Add(Processor.Create(OpCodes.Ret));
        return this;
    }

    // public MethodBodyBuilder Merge(InstructionSet instructionSet)
    // {
    //     exceptionHandlers.AddRange(instructionSet.ExceptionHandlers);
    //     instructions.AddRange(instructionSet.Instructions);
    //
    //     return this;
    // }

    // public InstructionSet Build()
    // {
    //     return new InstructionSet()
    //     {
    //         Instructions = instructions.ToArray(),
    //         ExceptionHandlers = exceptionHandlers.ToArray()
    //     };
    // }
}
