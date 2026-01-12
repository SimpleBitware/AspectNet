using System;

namespace SimpleBitware.AspectNet.Runtime.Aspects;

public sealed class AspectDescriptor(Type aspectType, params object[] args)
{
    public Type AspectType { get; } = aspectType;
    public object[] Arguments { get; } = args;
}