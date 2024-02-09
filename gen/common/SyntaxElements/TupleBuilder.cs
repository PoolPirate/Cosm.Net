using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosm.Net.Generators.Common.SyntaxElements;

public class TupleElement : ISyntaxBuilder
{
    public string Type { get; }
    public string? Name { get; }

    public TupleElement(string type, string? name)
    {
        Type = type;
        Name = name;
    }

    public SyntaxId GetSyntaxId()
        => new SyntaxId(HashCode.Combine(
            Type,
            Name
        ));
}

public class TupleBuilder : ITypeBuilder
{
    public string TypeName => Build();

    private readonly List<TupleElement> _elements;

    public TupleBuilder()
    {
        _elements = [];
    }

    public TupleBuilder AddElement(string type, string? name = null)
    {
        _elements.Add(new TupleElement(type, name));
        return this;
    }

    public string Build()
    {
        if (_elements.Count == 0)
        {
            throw new InvalidOperationException("Tuple requires at least one element");
        }

        return 
            $"({String.Join(",", _elements.Select(element => $"{element.Type}{(element.Name is null ? "" : $" {element.Name}")}"))})";
    }
    public SyntaxId GetSyntaxId()
           => GetContentId().Combine(new SyntaxId(HashCode.Combine(TypeName)));

    public SyntaxId GetContentId()
    {
        var innerSyntaxId = new SyntaxId(HashCode.Combine(
            nameof(TupleBuilder)
        ));

        foreach(var syntaxId in _elements.Select(x => x.GetSyntaxId()))
        {
            innerSyntaxId = innerSyntaxId.Combine(syntaxId);
        }
      
        return innerSyntaxId;
    }
}
