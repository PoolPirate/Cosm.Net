using Microsoft.CodeAnalysis;
using System.Text;
using System.Xml;

namespace Cosm.Net.Generators;
public static class CommentUtils
{
    public static bool TryGetSummary(ISymbol symbol, out string? summary)
    {
        var comment = symbol.GetDocumentationCommentXml();

        if (string.IsNullOrWhiteSpace(comment))
        {
            summary = null;
            return false;
        }

        var doc = new XmlDocument();
        doc.LoadXml(comment);

        var summaryElements = doc.GetElementsByTagName("summary");

        if (summaryElements.Count == 0)
        {
            summary = null;
            return false;
        }

        var summaryText = summaryElements[0].InnerText;

        if (string.IsNullOrWhiteSpace(summaryText))
        {
            summary = null;
            return false;
        }

        summary = summaryText!.Trim(' ', '\n', '\r');
        return true;
    }

    public static string MakeParamComment(string paramName, string content) 
        => $"""
           /// <param name="{paramName}">
           {EscapeLines(content)}
           /// </param>
           """;

    public static string MakeSummaryComment(string content)
        => $"""
           /// <summary>
           {EscapeLines(content)}
           /// </summary>
           """;

    public static string EscapeLines(string content)
    {
        var lines = content.Split('\n');
        var lineBuilder = new StringBuilder();

        foreach(var line in lines)
        {
            lineBuilder.AppendLine($"""/// {line}""");
        }

        return lineBuilder.ToString().Trim('\n', '\r', ' ');
    }
}
