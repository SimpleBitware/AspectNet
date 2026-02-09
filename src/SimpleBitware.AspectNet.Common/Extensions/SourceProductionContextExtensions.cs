using Microsoft.CodeAnalysis;

namespace SimpleBitware.AspectNet.Common.Extensions;

public static class SourceProductionContextExtensions
{
    extension(SourceProductionContext context)
    {
        public void WriteLine(string message, DiagnosticSeverity  diagnosticSeverity = DiagnosticSeverity.Info)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "ASPNETGEN",
                        "AspectNet Generator",
                        message,
                        "AspectNet",
                        diagnosticSeverity,
                        isEnabledByDefault: true),
                    Location.None));

        }
    }
}
