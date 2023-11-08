using Cosm.Net.Generators.SourceGeneratorKit;
using Microsoft.CodeAnalysis;
using System;

namespace Cosm.Net.Generators;

[Generator]
public class ModuleQueryClientGenerator : ISourceGenerator
{
    private readonly ClassesWithInterfacesReceiver QueryTypeReceiver = new ClassesWithInterfacesReceiver("ICosmModule");

    public ModuleQueryClientGenerator()
    {
    }

    public void Initialize(GeneratorInitializationContext context) 
        => context.RegisterForSyntaxNotifications(() => QueryTypeReceiver);
    public void Execute(GeneratorExecutionContext context)
    {
        if(context.SyntaxContextReceiver is not SyntaxReceiver)
        {
            return;
        }

        foreach(var moduleType in QueryTypeReceiver.Classes)
        {
            string code = QueryModuleProcessor.GetQueryModuleGeneratedCode(moduleType);

            try
            {
                context.AddSource($"{moduleType.Name}.generated.cs", code);
            }
            catch(ArgumentException)
            {
            }
        }
    }
}
