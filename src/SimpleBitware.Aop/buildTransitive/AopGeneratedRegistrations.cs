// This file is injected into consumer projects at design time.
// It provides IntelliSense for AOP extension points.
// The generator will extend these partial classes.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleBitware.Aop.Generated;

/// <summary>
/// This is a stub class the generator will extend.
/// </summary>
public static partial class AopGeneratedRegistrations
{
    /// <summary>
    /// Adds AOP support. The real implementation is generated at compile time.
    /// </summary>
    public static IServiceCollection AddAop(this IServiceCollection services)
        => throw new NotImplementedException();
}