namespace SimpleBitware.AspectNet.Abstractions.Attributes;

/// <summary>
/// Marks a class, method, property, or constructor to be excluded from AspectNet weaving.
/// Classes, methods, or members decorated with this attribute will not have aspect transformations applied.
/// </summary>
[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
public sealed class AspectNetExcludeAttribute : Attribute { }
