using System.Text;
using Mono.Cecil;

namespace SimpleBitware.AspectNet.Cecil.Debugging;

/// <summary>
/// Provides extension methods for debugging and inspecting Mono.Cecil module definitions.
/// </summary>
/// <remarks>
/// This class contains utility methods for dumping module contents to strings.
/// </remarks>
internal static class ModuleDefinitionExtensions
{
    /// <summary>
    /// Dumps the entire module definition to a formatted string representation.
    /// </summary>
    /// <param name="module">The module definition to dump.</param>
    /// <returns>A string containing the formatted representation of all types and methods in the module.</returns>
    /// <remarks>
    /// This method iterates through all types in the module and their methods,
    /// generating a human-readable dump that includes type names and method signatures
    /// along with their IL instructions.
    /// </remarks>
    public static string DumpModule(this ModuleDefinition module)
    {
        var sb = new StringBuilder();

        foreach (var type in module.Types)
        {
            sb.AppendLine($"// Type: {type.FullName}");

            foreach (var method in type.Methods)
            {
                sb.AppendLine($"\n.method {method.FullName}");
                sb.AppendLine(method.DumpMethod());
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Dumps the IL instructions of a method to a formatted string representation.
    /// </summary>
    /// <param name="method">The method definition to dump.</param>
    /// <returns>A string containing the formatted IL instructions with offsets and operands.</returns>
    /// <remarks>
    /// This method generates a disassembly-like output showing each IL instruction
    /// with its offset, opcode, and operand. This is useful for debugging the
    /// generated IL code during aspect weaving operations.
    /// </remarks>
    private static string DumpMethod(this MethodDefinition method)
    {
        var sb = new StringBuilder();

        if (method.HasBody)
        {
            foreach (var instr in method.Body.Instructions)
            {
                sb.AppendLine($"{instr.Offset:X4}: {instr.OpCode} {instr.Operand}");
            }   
        }

        return sb.ToString();
    }
}
