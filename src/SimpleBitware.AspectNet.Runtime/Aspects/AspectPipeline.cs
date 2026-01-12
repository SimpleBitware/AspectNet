using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleBitware.AspectNet.Runtime.Aspects;

public sealed class AspectPipeline : IAspectPipeline
{
    private readonly IAspectRegistry _registry;
    private readonly IServiceProvider _provider;

    public AspectPipeline(IAspectRegistry registry, IServiceProvider provider)
    {
        _registry = registry;
        _provider = provider;
    }

    // ------------------------------------------------------------
    // SYNC PIPELINE
    // ------------------------------------------------------------
    public object? Invoke(
        string methodId,
        object? target,
        object?[] args,
        Func<object?> invokeInner)
    {
        var descriptors = _registry.GetDescriptors(methodId);

        var aspects = descriptors
            .Select(d => (IMethodAspect)ActivatorUtilities.CreateInstance(_provider, d.AspectType, d.Arguments))
            .ToList();

        var ctx = new MethodContext
        {
            MethodId = methodId,
            Target = target,
            Arguments = args
        };

        foreach (var a in aspects)
            a.OnBefore(ctx);

        try
        {
            var result = invokeInner();
            ctx.ReturnValue = result;

            foreach (var a in aspects)
                a.OnSuccess(ctx);

            return result;
        }
        catch (Exception ex)
        {
            foreach (var a in aspects)
                a.OnException(ctx, ex);

            throw;
        }
    }

    // ------------------------------------------------------------
    // ASYNC PIPELINE
    // ------------------------------------------------------------
    public async Task<T> InvokeAsync<T>(
        string methodId,
        object? target,
        object?[] args,
        Func<Task<T>> invokeInnerAsync)
    {
        var descriptors = _registry.GetDescriptors(methodId);

        var aspects = descriptors
            .Select(d => (IMethodAspect)ActivatorUtilities.CreateInstance(_provider, d.AspectType, d.Arguments))
            .ToList();

        var ctx = new MethodContext
        {
            MethodId = methodId,
            Target = target,
            Arguments = args
        };

        foreach (var a in aspects)
            a.OnBefore(ctx);

        try
        {
            var result = await invokeInnerAsync().ConfigureAwait(false);
            ctx.ReturnValue = result;

            foreach (var a in aspects)
                a.OnSuccess(ctx);

            return result;
        }
        catch (Exception ex)
        {
            foreach (var a in aspects)
                a.OnException(ctx, ex);

            throw;
        }
    }

    public async Task InvokeAsync(
        string methodId,
        object? target,
        object?[] args,
        Func<Task> invokeInnerAsync)
    {
        var descriptors = _registry.GetDescriptors(methodId);

        var aspects = descriptors
            .Select(d => (IMethodAspect)ActivatorUtilities.CreateInstance(_provider, d.AspectType, d.Arguments))
            .ToList();

        var ctx = new MethodContext
        {
            MethodId = methodId,
            Target = target,
            Arguments = args
        };

        foreach (var a in aspects)
            a.OnBefore(ctx);

        try
        {
            await invokeInnerAsync().ConfigureAwait(false);

            // No return value → set to null
            ctx.ReturnValue = null;

            foreach (var a in aspects)
                a.OnSuccess(ctx);
        }
        catch (Exception ex)
        {
            foreach (var a in aspects)
                a.OnException(ctx, ex);

            throw;
        }
    }
}
