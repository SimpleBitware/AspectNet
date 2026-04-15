using Mono.Cecil;

namespace SimpleBitware.AspectNet.Cecil.Runtime;

/// <summary>
/// Prevents duplicate attribute types from different levels
/// </summary>
internal class AttributeTypeComparer : IEqualityComparer<CustomAttribute>
{
    public bool Equals(CustomAttribute? x, CustomAttribute? y) 
    {
        if (ReferenceEquals(x, y)) return true;
        if (x == null || y == null) return false;

        var tx = x.AttributeType;
        var ty = y.AttributeType;

        // Compare Namespace and Name to avoid Assembly Versioning issues
        return tx.Name == ty.Name && tx.Namespace == ty.Namespace;
    }

    public int GetHashCode(CustomAttribute obj) 
    {
        // Combine hashes of Name and Namespace
        unchecked 
        {
            var hash = 17;
            hash = hash * 23 + (obj.AttributeType.Name?.GetHashCode() ?? 0);
            hash = hash * 23 + (obj.AttributeType.Namespace?.GetHashCode() ?? 0);
            return hash;
        }
    }
}
