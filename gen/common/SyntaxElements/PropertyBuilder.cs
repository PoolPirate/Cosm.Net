using Cosm.Net.Generators.Common.Util;
using System;
using System.Text;

namespace Cosm.Net.Generators.Common.SyntaxElements;
public enum PropertyVisibility
{
    Public,
    Private,
    Internal
}
public enum SetterVisibility
{
    Public,
    Private,
    Init
}
public class PropertyBuilder : ISyntaxBuilder
{
    public string Type { get; private set; }
    public string Name { get; private set; }
    public string? DefaultValue { get; private set; }

    private PropertyVisibility _visibility = PropertyVisibility.Public;
    private SetterVisibility _setterVisibility = SetterVisibility.Public;
    private bool _isRequired = false;

    private string? _jsonPropertyName;
    private string? _summaryComment;

    public PropertyBuilder(string type, string name)
    {
        Type = type;
        Name = name;
    }

    public PropertyBuilder WithVisibility(PropertyVisibility visibility)
    {
        _visibility = visibility;
        return this;
    }

    public PropertyBuilder WithSetterVisibility(SetterVisibility setterVisibility)
    {
        _setterVisibility = setterVisibility;
        return this;
    }

    public PropertyBuilder WithIsRequired(bool isRequired = true)
    {
        _isRequired = isRequired;
        return this;
    }

    public PropertyBuilder WithSummaryComment(string summaryComment)
    {
        _summaryComment = summaryComment;
        return this;
    }

    public PropertyBuilder WithJsonPropertyName(string jsonPropertyName)
    {
        _jsonPropertyName = jsonPropertyName;
        return this;
    }

    public PropertyBuilder WithDefaultValue(string? defaultValue)
    {
        DefaultValue = defaultValue;
        return this;
    }

    public string Build()
    {
        var headerSb = new StringBuilder();

        if(_summaryComment is not null)
        {
            headerSb.AppendLine(CommentUtils.MakeSummaryComment(_summaryComment));
        }
        if(_jsonPropertyName is not null)
        {
            headerSb.AppendLine($"[System.Text.Json.Serialization.JsonPropertyName(\"{_jsonPropertyName}\")]");
        }

        return $$"""
            {{headerSb}} {{_visibility.ToString().ToLower()}} {{(_isRequired ? "required" : "")}} {{Type}} {{Name}} { get; {{(_setterVisibility == SetterVisibility.Init
                    ? "init"
                    : $"{_setterVisibility.ToString().ToLower()} set")}}; } {{(DefaultValue is not null ? $"= {DefaultValue};" : "")}}
            """;
    }

    public override int GetHashCode()
        => System.HashCode.Combine(
            Type,
            Name
        );
    public SyntaxId GetSyntaxId()
    {
        int hashCode = HashCode.Combine(
                Type,
                Name,
                _visibility,
                _setterVisibility,
                _isRequired,
                _jsonPropertyName
        );

        return new SyntaxId(hashCode);
    }
}
