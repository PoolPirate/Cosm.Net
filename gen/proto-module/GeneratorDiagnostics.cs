using Microsoft.CodeAnalysis;

namespace Cosm.Net.Generators.Proto;
public static class GeneratorDiagnostics
{
    private static class DiagnosticCategory
    {
        public const string Usage = "Usage";
    }

    public static readonly DiagnosticDescriptor ModuleClassShouldBeInternal = new DiagnosticDescriptor(
        "PG0000",
        "Module classes should be internal",
        "Module classes should be set to internal as they are meant to be used via a generated interface from the outside",
        DiagnosticCategory.Usage,
        DiagnosticSeverity.Warning,
        true
    );
}
