using Cosm.Net.Generators.Common.SourceGeneratorKit;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Cosm.Net.Generators;
public class MsgClassesReceiver : SyntaxReceiver
{
    private readonly List<INamedTypeSymbol> MsgTypeCandidates = [];
    private readonly List<INamedTypeSymbol> MsgResponseCandidates = [];

    public override bool CollectClassSymbol { get; } = true;

    protected override bool ShouldCollectClassSymbol(INamedTypeSymbol classSymbol)
    {
        if (IsMsgCandidate(classSymbol)) 
        {
            var matchingMsgResponseType = MsgResponseCandidates.Find(x => AreMatchingCandidates(classSymbol, x));

            if (matchingMsgResponseType is not null)
            {
                MsgResponseCandidates.Remove(matchingMsgResponseType);
                Classes.Add(classSymbol);
            }
            else
            {
                MsgTypeCandidates.Add(classSymbol);
            }
        }
        else if (IsMsgResponseCandidate(classSymbol))
        {
            var matchingMsgType = MsgTypeCandidates.Find(x => AreMatchingCandidates(x, classSymbol));

            if (matchingMsgType is not null)
            {
                MsgTypeCandidates.Remove(matchingMsgType);
                Classes.Add(matchingMsgType);
            } 
            else
            {
                MsgResponseCandidates.Add(classSymbol);
            }
        }

        return false;
    }

    private bool IsMsgCandidate(INamedTypeSymbol classSymbol)
      => classSymbol.Name.StartsWith("Msg")
            && !classSymbol.Name.EndsWith("Response");

    private bool IsMsgResponseCandidate(INamedTypeSymbol classSymbol)
      => classSymbol.Name.StartsWith("Msg")
            && classSymbol.Name.EndsWith("Response");

    private bool AreMatchingCandidates(INamedTypeSymbol msgCandidate, INamedTypeSymbol msgResponseCandidate)
        => msgCandidate.Name == msgResponseCandidate.Name.Substring(0, msgResponseCandidate.Name.Length - "Response".Length)
            && msgCandidate.ContainingAssembly.Equals(msgResponseCandidate.ContainingAssembly, SymbolEqualityComparer.Default)
            && msgCandidate.ContainingNamespace.Equals(msgResponseCandidate.ContainingNamespace, SymbolEqualityComparer.Default);
}
