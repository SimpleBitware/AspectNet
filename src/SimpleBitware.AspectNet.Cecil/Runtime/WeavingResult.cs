namespace SimpleBitware.AspectNet.Cecil.Runtime;

/// <summary>
/// Represents the result of a weaving operation.
/// </summary>
public record WeavingResult
{
    /// <summary>
    /// Gets the list of items that were cached during the weaving process.
    /// </summary>
    public string[] CachedItems { get; init; } = [];
    
    /// <summary>
    /// Gets the file name of the woven assembly, if available.
    /// </summary>
    public string? AssemblyFileName { get; init; }
    
    /// <summary>
    /// Gets the file name of the program database (PDB) file associated with the woven assembly, if available.
    /// </summary>
    public string? PdbFileName { get; init; }
}
