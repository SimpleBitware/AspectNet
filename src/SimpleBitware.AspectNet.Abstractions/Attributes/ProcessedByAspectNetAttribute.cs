namespace SimpleBitware.AspectNet.Abstractions.Attributes;

/// <summary>
/// Marks a class, method, property, or constructor as having been processed by AspectNet weaving.
/// This attribute is automatically added during the weaving process to indicate that aspect transformations have been applied.
/// </summary>
[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
public sealed class ProcessedByAspectNetAttribute : Attribute { }
