using Mono.Cecil;

namespace SimpleBitware.AspectNet.Cecil.Runtime;

/// <summary>
/// Provides equality comparison for custom attribute instances based on reference equality.
/// </summary>
/// <remarks>
/// This comparer is used to deduplicate custom attributes by comparing them by reference
/// rather than by value, which is important for aspect attribute processing.
/// </remarks>
internal class AttributeInstanceComparer : IEqualityComparer<CustomAttribute>
{
    /// <summary>
    /// Determines whether two custom attributes are equal by reference.
    /// </summary>
    /// <param name="x">The first custom attribute to compare.</param>
    /// <param name="y">The second custom attribute to compare.</param>
    /// <returns><c>true</c> if both attributes are the same reference; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method uses reference equality to ensure that duplicate attributes
    /// are properly identified and removed during aspect processing.
    /// </remarks>
    public bool Equals(CustomAttribute x, CustomAttribute y) => ReferenceEquals(x, y);
    
    /// <summary>
    /// Returns the hash code for a custom attribute.
    /// </summary>
    /// <param name="obj">The custom attribute to get the hash code for.</param>
    /// <returns>The hash code of the custom attribute object.</returns>
    /// <remarks>
    /// This method delegates to the object's default hash code implementation,
    /// which is based on reference identity for consistency with the Equals method.
    /// </remarks>
    public int GetHashCode(CustomAttribute obj) => obj.GetHashCode();
}
