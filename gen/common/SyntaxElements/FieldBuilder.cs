using System;

namespace Cosm.Net.Generators.Common.SyntaxElements;
public enum FieldVisibility
{
    Public,
    Private,
    Internal
}
public class FieldBuilder : ISyntaxBuilder
{
    public string Type { get; }
    public string Name { get; }

    private FieldVisibility _visibility = FieldVisibility.Private;
    private bool _isReadonly = true;
    private bool _isStatic = false;

    public FieldBuilder(string type, string name)
    {
        Type = type;
        Name = name;
    }

    public FieldBuilder WithVisibility(FieldVisibility visibility)
    {
        _visibility = visibility;
        return this;
    }

    public FieldBuilder WithIsReadonly(bool isReadonly)
    {
        _isReadonly = isReadonly;
        return this;
    }

    public FieldBuilder WithIsStatic(bool isStatic = true)
    {
        _isStatic = isStatic;
        return this;
    }

    public string Build()
        => $$"""
            {{_visibility.ToString().ToLower()}}{{(_isStatic ? " static" : "")}}{{(_isReadonly ? " readonly" : "")}} {{Type}} {{Name}};
            """;

    public SyntaxId GetSyntaxId()
    {
        int hashCode = HashCode.Combine(
            nameof(FieldBuilder),
            Type,
            Name,
            _visibility,
            _isReadonly
        );
        return new SyntaxId(hashCode);
    }
}
