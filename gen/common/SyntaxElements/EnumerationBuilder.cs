using Cosm.Net.Generators.Common.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosm.Net.Generators.Common.SyntaxElements;
public class EnumerationEntry
{
    public string Value { get; }
    public string? JsonPropertyName { get; }
    public string? SummaryComment { get; }

    public EnumerationEntry(string value, string? jsonPropertyName, string? summaryComment)
    {
        Value = value;
        JsonPropertyName = jsonPropertyName;
        SummaryComment = summaryComment;
    }
}
public class EnumerationBuilder : ISyntaxBuilder
{
    private readonly List<EnumerationEntry> _enumerationValues;
    private readonly string _name;

    private string? _summaryComment;

    public EnumerationBuilder(string name)
    {
        _enumerationValues = [];
        _name = name;
    }

    public EnumerationBuilder AddValue(string value, string? jsonPropertyName, string? summaryComment)
    {
        _enumerationValues.Add(new EnumerationEntry(value, jsonPropertyName, summaryComment));
        return this;
    }

    public EnumerationBuilder WithSummaryComment(string summaryComment)
    {
        _summaryComment = summaryComment;
        return this;
    }

    public string Build()
    {
        var valueSb = new StringBuilder();

        foreach(var entry in _enumerationValues)
        {
            valueSb.AppendLine(
                $$"""
                {{(entry.SummaryComment is not null ? CommentUtils.MakeSummaryComment(entry.SummaryComment) : "")}}
                {{(entry.JsonPropertyName is not null 
                    ? $"[System.Text.Json.Serialization.JsonPropertyName(\"{entry.JsonPropertyName}\")]" 
                    : "")}}
                {{entry.Value}},
                """);
        }

        return
            $$"""
            {{(_summaryComment is not null ? CommentUtils.MakeSummaryComment(_summaryComment) : "")}}
            public enum {{_name}} {
            {{valueSb}}
            }
            """;
    }
}
