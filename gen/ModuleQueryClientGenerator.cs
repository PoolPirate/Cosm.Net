using Cosm.Net.Generators.SourceGeneratorKit;
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
    {
//#if DEBUG
//        if(!Debugger.IsAttached)
//        {
//            Debugger.Launch();
//        }
//#endif

        context.RegisterForSyntaxNotifications(() => new AggregateSyntaxContextReceiver(QueryTypeReceiver, MsgClassesReceiver));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if(context.SyntaxContextReceiver is null)
        {
            return;
        }

        foreach(var moduleType in QueryTypeReceiver.Classes)
        {
            var queryClientType = QueryModuleProcessor.GetQueryClientType(moduleType);
            var msgTypes = MsgClassesReceiver.Classes
                .Where(x => x.ContainingNamespace.Equals(queryClientType.ContainingNamespace, SymbolEqualityComparer.Default));

            foreach(var msgType in msgTypes)
            {
                try
                {
                    string msgCode = TxMsgProcessor.GetTxMsgGeneratedCode(msgType);
                    context.AddSource($"{msgType.ContainingNamespace}.{msgType.Name}.generated.cs", msgCode);
                }
                catch(Exception ex)
                {

                }
            }

            string moduleCode = QueryModuleProcessor.GetQueryModuleGeneratedCode(moduleType, queryClientType);

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
