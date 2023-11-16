using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Xml;

namespace Cosm.Net.Generators.Common.Util;
public static class CommentUtils
{
    public static bool TryGetSummary(ISymbol symbol, out string? summary)
    {
        string? comment = symbol.GetDocumentationCommentXml();

        if(System.String.IsNullOrWhiteSpace(comment))
        {
            summary = null;
            return false;
        }

        var doc = new XmlDocument();
        doc.LoadXml(comment);

        var summaryElements = doc.GetElementsByTagName("summary");

        if(summaryElements.Count == 0)
        {
            summary = null;
            return false;
        }

        string summaryText = summaryElements[0].InnerText;

        if(System.String.IsNullOrWhiteSpace(summaryText))
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
        var lines = content.Split('\n').Where(x => x.Trim('\n', '\r', ' ').Length > 0);
        var lineBuilder = new StringBuilder();

        foreach(string? line in lines)
        {
            _ = lineBuilder.AppendLine($"""/// {line.Trim('\n', '\r', ' ')}""");
        }

        return lineBuilder.ToString().Trim('\n', '\r', ' ');
    }
}
