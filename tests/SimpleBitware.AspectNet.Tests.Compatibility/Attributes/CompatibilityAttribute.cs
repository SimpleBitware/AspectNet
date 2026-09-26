using System;
using SimpleBitware.AspectNet.Abstractions.Attributes;

namespace SimpleBitware.AspectNet.Tests.Compatibility.Attributes;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor, AllowMultiple = true)]
public class CompatibilityAttribute : AbstractAspectNetAttribute
{
    
}
