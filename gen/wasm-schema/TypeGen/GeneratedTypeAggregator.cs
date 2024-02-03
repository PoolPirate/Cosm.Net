using Cosm.Net.Generators.Common.SyntaxElements;
using Cosm.Net.Generators.CosmWasm.Models;

namespace Cosm.Net.Generators.CosmWasm.TypeGen;
public class GeneratedTypeAggregator
{
    private Dictionary<string, int> _typeNameOccurences;
    private Dictionary<SyntaxId, ITypeBuilder> _types;

    public GeneratedTypeAggregator()
    {
        _typeNameOccurences = [];
        _types = [];
    }

    public void Reset()
    {
        _typeNameOccurences = [];
        _types = [];
    }

    public GeneratedTypeHandle GenerateTypeHandle(ITypeBuilder type)
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

    public IEnumerable<ITypeBuilder> GetGeneratedTypes()
        => _types.Values;
}
