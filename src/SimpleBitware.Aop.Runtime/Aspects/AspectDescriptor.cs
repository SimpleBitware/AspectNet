using System;

namespace SimpleBitware.Aop.Runtime.Aspects;

public sealed class AspectDescriptor(Type aspectType, params object[] args)
{
    public Type AspectType { get; } = aspectType;
    public object[] Arguments { get; } = args;
}