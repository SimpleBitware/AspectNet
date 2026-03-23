using System.Text;
using Mono.Cecil;

namespace SimpleBitware.AspectNet.Debugging;

internal static class ModuleDefinitionExtensions
{
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

    private static string DumpMethod(this MethodDefinition method)
    {
        var sb = new StringBuilder();

        foreach (var instr in method.Body.Instructions)
        {
            sb.AppendLine($"{instr.Offset:X4}: {instr.OpCode} {instr.Operand}");
        }

        return sb.ToString();
    }
}
