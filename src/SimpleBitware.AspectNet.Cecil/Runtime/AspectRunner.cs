using System.Reflection;
using System.Runtime.ExceptionServices;
using SimpleBitware.AspectNet.Abstractions;
using SimpleBitware.AspectNet.Abstractions.Attributes;

namespace SimpleBitware.AspectNet.Cecil.Runtime;

public static class AspectRunner
{
    public static T? Execute<T, TAspect>(object target, MethodInfo method, AspectNetAttributeContext context, params object[] args)
        where TAspect : class, IAspectNetAttribute, new()
    {
        var aspect = AspectNetDependencyInjection.GetRequiredService<TAspect>();

        try
        {
            aspect.OnEntry(context);
            var result = method.Invoke(target, args);

            if (result is Task task)
                return (T)InternalExecuteAsync(task, context, aspect, method);

            if (method.ReturnType == typeof(ValueTask))
                return (T)InternalHandleValueTask(result!, context, aspect, method);

            if (!IsAsync(method.ReturnType))
            {
                context.ReturnValue = result;
                aspect.OnSuccess(context);
            }
        }
        catch (Exception ex)
        {
            HandleException(ex, context, aspect);
        }
        finally
        {
            if (!IsAsync(method.ReturnType))
                aspect.OnExit(context);
        }

        return (T?)context.ReturnValue;
    }

    private static object InternalHandleValueTask(object vt, AspectNetAttributeContext context, IAspectNetAttribute aspect, MethodInfo method)
    {
        var task = GetTaskFromValueTask(vt, method.ReturnType);
        var taskWrapper = InternalExecuteAsync(task, context, aspect, method);
        return method.ReturnType.IsGenericType
            ? Activator.CreateInstance(method.ReturnType, taskWrapper)!
            : new ValueTask((Task)taskWrapper);
    }

    private static object InternalExecuteAsync(Task task, AspectNetAttributeContext context, IAspectNetAttribute aspect, MethodInfo method)
    {
        if (!IsValueTask(method.ReturnType))
            return ExecuteAsyncVoid(task, context, aspect);

        var resultType = method.ReturnType.GetGenericArguments()[0];
        var mi = typeof(AspectRunner).GetMethod(nameof(ExecuteAsyncGeneric), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(resultType);
        return mi.Invoke(null, [task, context, aspect])!;
    }

    private static async Task<T?> ExecuteAsyncGeneric<T>(Task<T> task, AspectNetAttributeContext context, IAspectNetAttribute aspect)
    {
        try
        {
            var result = await task;
            context.ReturnValue = result;
            aspect.OnSuccess(context);
        }
        catch (Exception ex)
        {
            HandleException(ex, context, aspect);
        }
        finally
        {
            aspect.OnExit(context);
        }

        return (T?)context.ReturnValue;
    }

    private static async Task ExecuteAsyncVoid(Task task, AspectNetAttributeContext context, IAspectNetAttribute aspect)
    {
        try
        {
            await task;
            aspect.OnSuccess(context);
        }
        catch (Exception ex)
        {
            HandleException(ex, context, aspect);
        }
        finally
        {
            aspect.OnExit(context);
        }
    }

    private static void HandleException(Exception ex, AspectNetAttributeContext context, IAspectNetAttribute aspect)
    {
        var actual = (ex is TargetInvocationException tex) ? tex.InnerException : ex;
        context.Exception = actual;
        aspect.OnException(context);

        if (actual != null && ReferenceEquals(context.Exception, actual))
            ExceptionDispatchInfo.Capture(actual).Throw();

        if (context.Exception != null)
            throw context.Exception;
    }

    private static Task GetTaskFromValueTask(object vt, Type returnType)
    {
        if (vt is ValueTask valueTask)
            return valueTask.AsTask();

        var asTaskMethod = returnType.GetMethod(nameof(ValueTask.AsTask));
        if (asTaskMethod == null)
            throw new InvalidOperationException($"Could not find {nameof(ValueTask.AsTask)} on type {returnType.FullName}");

        return (Task)asTaskMethod.Invoke(vt, null)!;
    }

    private static bool IsValueTask(Type type)
    {
        return (type == typeof(ValueTask)) ||
               (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>));
    }

    private static bool IsAsync(Type type)
    {
        return typeof(Task).IsAssignableFrom(type) || IsValueTask(type);
    }
}
