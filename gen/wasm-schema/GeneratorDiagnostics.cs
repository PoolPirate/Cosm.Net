using Microsoft.CodeAnalysis;

namespace Cosm.Net.Generators.CosmWasm;
public static class GeneratorDiagnostics
{
    private static class DiagnosticCategory
    {
        public const string Usage = "Usage";
    }

    public static readonly DiagnosticDescriptor SchemaFileNotFound = new DiagnosticDescriptor(
        "CWG0001", 
        "Schema file not found",
        "Schema file not found",
        DiagnosticCategory.Usage, 
        DiagnosticSeverity.Warning, 
        true
    );

        public static readonly DiagnosticDescriptor SchemaFileMalformed = new DiagnosticDescriptor(
        "CWG0002",
        "Schema file could not be parsed",
        "Schema file could not be parsed. An exception occured: {0}.",
        DiagnosticCategory.Usage,
        DiagnosticSeverity.Warning,
        true
);
}
