using System;

namespace SimpleBitware.Aop.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class LogAttribute(string? category = null) : Attribute
{
    public string? Category { get; } = category;
}