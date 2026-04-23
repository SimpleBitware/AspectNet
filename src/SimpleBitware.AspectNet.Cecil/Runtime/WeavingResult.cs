namespace SimpleBitware.AspectNet.Cecil.Runtime;

public record WeavingResult
{
    public string[] CachedItems { get; init; } = [];
    public string? AssemblyFileName { get; init; }
    public string? PdbFileName { get; init; }
}
