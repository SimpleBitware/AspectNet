using Mono.Cecil;

namespace SimpleBitware.AspectNet.Cecil.Runtime;

internal class AttributeInstanceComparer : IEqualityComparer<CustomAttribute>
{
    // Check if they are the exact same instance in the metadata
    public bool Equals(CustomAttribute x, CustomAttribute y) => ReferenceEquals(x, y);
    public int GetHashCode(CustomAttribute obj) => obj.GetHashCode();
}
