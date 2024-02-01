using Cosm.Net.Generators.Common.SyntaxElements;
using Cosm.Net.Generators.CosmWasm.Models;

namespace Cosm.Net.Generators.CosmWasm.TypeGen;
public static class GeneratedTypeAggregator
{
    private static Dictionary<string, int> _typeNameOccurences = [];
    private static Dictionary<SyntaxId, ITypeBuilder> _types = [];

    public static void Reset()
    {
        _typeNameOccurences = [];
        _types = [];
    }

    public static GeneratedTypeHandle GenerateTypeHandle(ITypeBuilder type)
    {
        var syntaxId = type.GetSyntaxId();
        if(_types.TryGetValue(syntaxId, out _))
        {
            return new GeneratedTypeHandle(type.TypeName, null);
        }

        if(_typeNameOccurences.TryGetValue(type.TypeName, out int occurences))
        {
            _typeNameOccurences[type.TypeName] = occurences + 1;
        }
        else
        {
            _typeNameOccurences.Add(type.TypeName, 1);
        }

        _types.Add(syntaxId, type);

        return new GeneratedTypeHandle(
            occurences == 0 ? type.TypeName : $"{type.TypeName}{occurences}",
            null
        );
    }

    public static IEnumerable<ITypeBuilder> GetGeneratedTypes()
        => _types.Values;
}
