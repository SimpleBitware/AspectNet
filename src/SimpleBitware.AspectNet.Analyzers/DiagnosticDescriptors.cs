using Microsoft.CodeAnalysis;

namespace SimpleBitware.AspectNet.Analyzers;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor ClassMustBePartial = new(
        id: "ASPECT001",
        title: "Class must be partial",
        messageFormat: "Class '{0}' must be declared partial to enable AspectNet weaving",
        category: "AspectNet",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Classes using AspectNet attributes must be declared partial so the generator can emit weaving code.");
}
