using Cosm.Net.Generators.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosm.Net.Generators.Common.SyntaxElements;
public class EnumerationEntry
{
    public string Value { get; }
    public string? SummaryComment { get; }

    public EnumerationEntry(string value, string? summaryComment)
    {
        Value = value;
        SummaryComment = summaryComment;
    }
}
public class EnumerationBuilder : ITypeBuilder
{
    private readonly List<EnumerationEntry> _enumerationValues;
    private string _name;

    private string? _summaryComment;
    private string? _jsonConverter;

    public EnumerationBuilder(string name)
    {
        _enumerationValues = [];
        _name = name;
    }

    public EnumerationBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public EnumerationBuilder AddValue(string value, string? summaryComment)
    {
        _enumerationValues.Add(new EnumerationEntry(value, summaryComment));
        return this;
    }

    public EnumerationBuilder WithSummaryComment(string summaryComment)
    {
        _summaryComment = summaryComment;
        return this;
    }

    public EnumerationBuilder WithJsonConverter(string jsonConverter)
    {
        _jsonConverter = jsonConverter;
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
                {{entry.Value}},
                """);
        }
        
        return
            $$"""
            {{(_summaryComment is not null ? CommentUtils.MakeSummaryComment(_summaryComment) : "")}}
            {{(_jsonConverter is not null ? $"[global::System.Text.Json.Serialization.JsonConverterAttribute(typeof({_jsonConverter}))]" : "")}}
            public enum {{_name}} {
            {{valueSb}}
            }
            """;
    }

    public override int GetHashCode()
    {
        int hc = _enumerationValues.Count;

        foreach(int val in _enumerationValues.Select(x => x.Value.GetHashCode()))
        {
            hc = unchecked((hc * 314159) + val);
        }

        return HashCode.Combine(
            _name,
            hc
        );
    }
}
