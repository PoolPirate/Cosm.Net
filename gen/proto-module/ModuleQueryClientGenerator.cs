using Cosm.Net.Generators.Common.SourceGeneratorKit;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.Linq;

namespace Cosm.Net.Generators;

[Generator]
public class ModuleQueryClientGenerator : ISourceGenerator
{
    private readonly ClassesWithInterfacesReceiver QueryTypeReceiver = new ClassesWithInterfacesReceiver("ICosmModule");
    private readonly MsgClassesReceiver MsgClassesReceiver = new MsgClassesReceiver();

    public ModuleQueryClientGenerator()
    {
    }

    public void Initialize(GeneratorInitializationContext context) 
        => context.RegisterForSyntaxNotifications(() => new AggregateSyntaxContextReceiver(QueryTypeReceiver, MsgClassesReceiver));

    public void Execute(GeneratorExecutionContext context)
    {
        if(context.SyntaxContextReceiver is null)
        {
            return;
        }

        foreach(var moduleType in QueryTypeReceiver.Classes)
        {
            if (moduleType.DeclaredAccessibility != Accessibility.Internal)
            {
                context.ReportDiagnostic(Diagnostic.Create("CN0011", "Analyzer", "Module classes need to be internal", DiagnosticSeverity.Error, DiagnosticSeverity.Error, true, 7, location: moduleType.Locations[0]));
            }

            var queryClientType = QueryModuleProcessor.GetQueryClientType(moduleType);
            var msgTypes = MsgClassesReceiver.Classes
                .Where(x => x.ContainingNamespace.Equals(queryClientType.ContainingNamespace, SymbolEqualityComparer.Default));

            string moduleCode = QueryModuleProcessor.GetQueryModuleGeneratedCode(moduleType, queryClientType, msgTypes);

            try
            {
                context.AddSource($"{moduleType.Name}.generated.cs", moduleCode);
            }
            catch(ArgumentException)
            {
            }
        }
    }
}
