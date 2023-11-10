using Cosm.Net.Core.Msg;
using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace Cosm.Net.Generators;
public static class TxMsgProcessor
{
    public static string GetTxMsgGeneratedCode(INamedTypeSymbol msgType)
    {
        //DebuggerUtil.Attach();

        return 
            $$"""
            using {{typeof(ITxMessage).Namespace}};

            namespace {{msgType.ContainingAssembly.Name}}.{{ShortenNamespace(msgType.ContainingNamespace.ToString())}};

            public class {{msgType.Name}} : {{nameof(ITxMessage)}} {
                
            }
            """;
    }

    private static string ShortenNamespace(string name)
    {
        int prefixLength = name.IndexOf('.') + 1;
        string shortened = name.Substring(prefixLength, name.Length - prefixLength);
        return shortened;
    }
}
