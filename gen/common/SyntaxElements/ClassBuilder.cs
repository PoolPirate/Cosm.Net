using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosm.Net.Generators.Common.SyntaxElements;

public enum ClassVisibility
{
    Public,
    Internal
}
public class BaseType : ISyntaxBuilder
{
    public string Name { get; }
    public bool IsInterface { get; }

    public BaseType(string name, bool isInterface)
    {
        Name = name;
        IsInterface = isInterface;
    }

    public SyntaxId GetSyntaxId()
        => new SyntaxId(HashCode.Combine(
            Name,
            IsInterface
        ));
}
public class ClassBuilder : ITypeBuilder
{
    private readonly List<FunctionBuilder> _functions;
    private readonly List<PropertyBuilder> _properties;
    private readonly List<FieldBuilder> _fields;
    private readonly List<BaseType> _baseTypes;
    private string _name;
    private ClassVisibility _visibility = ClassVisibility.Public;
    private bool _isPartial = false;
    private bool _isAbstract = false;

    public ClassBuilder(string name)
    {
        _functions = [];
        _properties = [];
        _fields = [];
        _baseTypes = [];
        _name = name;
    }

    public IReadOnlyList<FieldBuilder> GetFields() => _fields;
    public IReadOnlyList<PropertyBuilder> GetProperties() => _properties;
    public IReadOnlyList<FunctionBuilder> GetFunctions() => _functions;

    public ClassBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    public ClassBuilder AddFunction(FunctionBuilder function)
    {
        _functions.Add(function);
        return this;
    }

    public ClassBuilder AddProperty(PropertyBuilder property)
    {
        _properties.Add(property);
        return this;
    }

    public ClassBuilder AddField(FieldBuilder field)
    {
        _fields.Add(field);
        return this;
    }

    public ClassBuilder AddFunctions(IEnumerable<FunctionBuilder> functions)
    {
        _functions.AddRange(functions);
        return this;
    }

    public ClassBuilder AddBaseType(string name, bool isInterface)
    {
        _baseTypes.Add(new BaseType(name, isInterface));
        return this;
    }

    public ClassBuilder WithVisibility(ClassVisibility visibility)
    {
        _visibility = visibility;
        return this;
    }

    public ClassBuilder WithIsPartial(bool isPartial = true)
    {
        _isPartial = isPartial;
        return this;
    }

    public ClassBuilder WithIsAbstract(bool isAbstract = true)
    {
        _isAbstract = isAbstract;
        return this;
    }

    public string Build(bool generateFieldConstructor = false, bool generateInterface = false, string? interfaceName = null)
    {
        var bodySb = new StringBuilder();
        var baseTypeSb = new StringBuilder();

        foreach(var function in _functions)
        {
            bodySb.AppendLine(function.BuildMethodCode());
        }
        foreach(var property in _properties)
        {
            bodySb.AppendLine(property.Build());
        }
        foreach(var field in _fields)
        {
            bodySb.AppendLine(field.Build());
        }

        var orderedBaseTypes = _baseTypes.OrderBy(x => !x.IsInterface).ToList();

        if (generateInterface)
        {
            orderedBaseTypes.Add(new BaseType($"I{_name}", true));
        }

        for(int i = 0; i < orderedBaseTypes.Count; i++)
        {
            var baseType = orderedBaseTypes[i];

            if (i == 0)
            {
                baseTypeSb.Append(" : ");
            }

            baseTypeSb.Append(baseType.Name);

            if (i < orderedBaseTypes.Count - 1)
            {
                baseTypeSb.Append(", ");
            }
        }

        return 
            $$"""
            {{(!generateInterface ? ""
                : new InterfaceBuilder(interfaceName ?? $"I{_name}")
                    .AddFunctions(_functions)
                    .AddBaseTypes(_baseTypes)
                    .WithIsPartial(_isPartial)
                    .Build()
            )}}
            {{_visibility.ToString().ToLower()}}
            {{(_isPartial ? " partial" : "")}} 
            {{(_isAbstract ? " abstract" : "")}}
            class {{_name}} 
                {{baseTypeSb}}
            {
            {{(!generateFieldConstructor ? ""
                : new ConstructorBuilder(_name)
                    .AddInitializedFields(_fields)
                    .Build())}}
            {{bodySb}}
            }
            """;
    }

    public string Build() 
        => Build(false);

    public SyntaxId GetSyntaxId() 
        => GetContentId().Combine(new SyntaxId(HashCode.Combine(_name)));

    public SyntaxId GetContentId()
    {
        var innerSyntaxId = new SyntaxId(HashCode.Combine(
            nameof(ClassBuilder),
            _visibility,
            _isPartial,
            _isAbstract
        ));

        foreach(var syntaxId in _properties.Select(x => x.GetSyntaxId()))
        {
            innerSyntaxId = innerSyntaxId.Combine(syntaxId);
        }
        foreach(var syntaxId in _fields.Select(x => x.GetSyntaxId()))
        {
            innerSyntaxId = innerSyntaxId.Combine(syntaxId);
        }
        foreach(var syntaxId in _functions.Select(x => x.GetSyntaxId()))
        {
            innerSyntaxId = innerSyntaxId.Combine(syntaxId);
        }
        foreach(var syntaxId in _baseTypes.Select(x => x.GetSyntaxId()))
        {
            innerSyntaxId = innerSyntaxId.Combine(syntaxId);
        }

        return innerSyntaxId;
    }
}
