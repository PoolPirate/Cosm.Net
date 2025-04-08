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

        if(_typeNameOccurences.TryGetValue(type.TypeName.ToLower().Trim(), out int occurences))
        {
            _typeNameOccurences[type.TypeName.ToLower().Trim()] = occurences + 1;
        }
        else
        {
            _typeNameOccurences.Add(type.TypeName.ToLower().Trim(), 1);
        }

        string typeName = occurences == 0
                ? type.TypeName
                : $"{type.TypeName}{occurences}";

        _types.Add(syntaxId, type switch
        {
            ClassBuilder cb => cb.WithName(typeName),
            EnumerationBuilder eb => eb.WithName(typeName),
            _ => type
        });

        return new GeneratedTypeHandle(
            typeName,
            null
        );
    }

    public IEnumerable<ITypeBuilder> GetGeneratedTypes()
        => _types.Values;
}
