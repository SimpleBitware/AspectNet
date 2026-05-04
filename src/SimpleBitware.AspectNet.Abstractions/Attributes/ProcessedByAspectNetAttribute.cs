namespace SimpleBitware.AspectNet.Abstractions.Attributes;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
public sealed class ProcessedByAspectNetAttribute : Attribute { }
