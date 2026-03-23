using Mono.Cecil;

namespace SimpleBitware.AspectNet.Runtime.Cecil;

/// <summary>
/// Prevents duplicate attribute types from different levels
/// </summary>
internal class AttributeTypeComparer : IEqualityComparer<CustomAttribute>
{
    public bool Equals(CustomAttribute x, CustomAttribute y) => x.AttributeType.FullName == y.AttributeType.FullName;
    public int GetHashCode(CustomAttribute obj) => obj.AttributeType.FullName.GetHashCode();
}
