// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

/// <summary>
/// Indicates that a type or method has required members that must be initialized.
/// This attribute is used for compatibility with C# 11's required keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class RequiredMemberAttribute : Attribute
{
}

/// <summary>
/// Indicates that a type or method requires a specific compiler feature.
/// This attribute is used for compatibility with C# 11's compiler feature requirements.
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public sealed class CompilerFeatureRequiredAttribute(string featureName) : Attribute
{
    /// <summary>
    /// Gets the name of the required compiler feature.
    /// </summary>
    public string FeatureName { get; } = featureName;
    
    /// <summary>
    /// Gets or sets a value indicating whether the compiler feature is optional.
    /// </summary>
    public bool IsOptional { get; init; }
}

/// <summary>
/// Contains constants for compiler features used in required member validation.
/// </summary>
internal static class CompilerFeatureRequired
{
    /// <summary>
    /// The name of the RequiredMembers compiler feature.
    /// </summary>
    public const string RequiredMembers = nameof(RequiredMembers);
}
