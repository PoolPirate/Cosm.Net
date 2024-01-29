using Cosm.Net.Generators.Common.Util;
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
    private readonly List<ITypeBuilder> _innerTypes;

    private string _name;
    private ClassVisibility _visibility = ClassVisibility.Public;
    private bool _isPartial = false;
    private bool _isAbstract = false;

    private string? _jsonConverterType;
    private string? _summaryComment;

    string ITypeBuilder.TypeName => _name;

    public ClassBuilder(string name)
    {
        _functions = [];
        _properties = [];
        _fields = [];
        _baseTypes = [];
        _innerTypes = [];
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

    public ClassBuilder AddInnerType(ITypeBuilder typeBuilder)
    {
        _innerTypes.Add(typeBuilder);
        return this;
    }

    public ClassBuilder AddBaseType(string name, bool isInterface)
    {
        _baseTypes.Add(new BaseType(name, isInterface));
        return this;
    }

    public ClassBuilder WithJsonConverterType(string jsonConverterType)
    {
        _jsonConverterType = jsonConverterType;
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

    public ClassBuilder WithSummaryComment(string summaryComment)
    {
        _summaryComment = summaryComment;
        return this;
    }

    public string Build(bool generateFieldConstructor = false, bool generateInterface = false, string? interfaceName = null)
    {
        var bodySb = new StringBuilder();
        var baseTypeSb = new StringBuilder();

        foreach(var function in _functions)
        {
            _ = bodySb.AppendLine(function.BuildMethodCode());
        }
        foreach(var property in _properties)
        {
            _ = bodySb.AppendLine(property.Build());
        }
        foreach(var field in _fields)
        {
            _ = bodySb.AppendLine(field.Build());
        }
        foreach(var type in _innerTypes)
        {
            _ = bodySb.AppendLine(type.Build());
        }

        var orderedBaseTypes = _baseTypes.OrderBy(x => !x.IsInterface).ToList();

        if(generateInterface)
        {
            orderedBaseTypes.Add(new BaseType($"I{_name}", true));
        }

        for(int i = 0; i < orderedBaseTypes.Count; i++)
        {
            var baseType = orderedBaseTypes[i];

            if(i == 0)
            {
                _ = baseTypeSb.Append(" : ");
            }

            _ = baseTypeSb.Append(baseType.Name);

            if(i < orderedBaseTypes.Count - 1)
            {
                _ = baseTypeSb.Append(", ");
            }
        }

        var outputSb = new StringBuilder();

        if (generateInterface)
        {
            outputSb.AppendLine(
                new InterfaceBuilder(interfaceName ?? $"I{_name}")
                .AddFunctions(_functions)
                .AddBaseTypes(_baseTypes)
                .WithIsPartial(_isPartial)
                .Build());
        }

        if (_summaryComment is not null)
        {
            outputSb.AppendLine(
                CommentUtils.MakeSummaryComment(_summaryComment));
        }

        if (_jsonConverterType is not null)
        {
            outputSb.AppendLine($"[global::System.Text.Json.Serialization.JsonConverter(typeof({_jsonConverterType}))]");
        }

        outputSb.Append(_visibility.ToString().ToLower());

        if (_isPartial)
        {
            outputSb.Append(" partial");
        }
        if (_isAbstract)
        {
            outputSb.Append(" abstract");
        }
        outputSb.AppendLine($" class {_name} {baseTypeSb}");
        outputSb.AppendLine("{");

        if (generateFieldConstructor)
        {
            outputSb.AppendLine(
                new ConstructorBuilder(_name)
                    .AddInitializedFields(_fields)
                    .AddInitializedProperties(_properties)
                    .Build());
        }

        outputSb.AppendLine(bodySb.ToString().TrimEnd('\n', '\r'));
        outputSb.AppendLine("}");
        return outputSb.ToString();
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
            _isAbstract,
            _jsonConverterType
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
        foreach(var syntaxId in _innerTypes.Select(x => x.GetSyntaxId()))
        {
            innerSyntaxId = innerSyntaxId.Combine(syntaxId);
        }

        return innerSyntaxId;
    }
}
