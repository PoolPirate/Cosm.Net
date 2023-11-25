using Cosm.Net.Generators.Common.Util;
using System;

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

    public string Build() 
        => $$"""
            {{(_summaryComment is not null ? CommentUtils.MakeSummaryComment(_summaryComment) : "")}}
            {{(_jsonPropertyName is not null ? $"[System.Text.Json.Serialization.JsonPropertyName(\"{_jsonPropertyName}\")]" : "")}}
            {{_visibility.ToString().ToLower()}} {{(_isRequired ? "required" : "")}} {{Type}} {{Name}} { get; {{
            (_setterVisibility == SetterVisibility.Init 
                ? "init" 
                : $"{_setterVisibility.ToString().ToLower()} set")}}; }
            """;

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
