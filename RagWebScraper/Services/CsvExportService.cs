using System.Text;
using RagWebScraper.Models;

namespace RagWebScraper.Services;

/// <summary>
/// Utility service for exporting analysis results to CSV format.
/// </summary>
public sealed class CsvExportService : ICsvExportService
{
    public byte[] ExportPageSentiment(IEnumerable<AnalysisResult> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Source,PageSentimentScore");
        foreach (var r in results ?? Enumerable.Empty<AnalysisResult>())
        {
            var source = string.IsNullOrWhiteSpace(r.Url) ? r.FileName : r.Url;
            sb.AppendLine($"\"{Escape(source)}\",{r.PageSentimentScore}");
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] ExportCrossLinks(IEnumerable<LinkedPassage> links)
    {
        var sb = new StringBuilder();
        sb.AppendLine("SourceIdA,TextA,SourceIdB,TextB,Similarity");
        foreach (var l in links ?? Enumerable.Empty<LinkedPassage>())
        {
            sb.AppendLine($"\"{Escape(l.SourceIdA)}\",\"{Escape(l.TextA)}\",\"{Escape(l.SourceIdB)}\",\"{Escape(l.TextB)}\",{l.Similarity}");
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] ExportKeywordSentiment(IEnumerable<AnalysisResult> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Source,Keyword,SentimentScore");
        foreach (var r in results ?? Enumerable.Empty<AnalysisResult>())
        {
            var source = string.IsNullOrWhiteSpace(r.Url) ? r.FileName : r.Url;
            foreach (var kv in r.KeywordSentimentScores)
            {
                sb.AppendLine($"\"{Escape(source)}\",\"{Escape(kv.Key)}\",{kv.Value}");
            }
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] ExportKeywordFrequencies(IEnumerable<AnalysisResult> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Source,Keyword,Frequency");
        foreach (var r in results ?? Enumerable.Empty<AnalysisResult>())
        {
            var source = string.IsNullOrWhiteSpace(r.Url) ? r.FileName : r.Url;
            foreach (var kv in r.KeywordFrequencies)
            {
                sb.AppendLine($"\"{Escape(source)}\",\"{Escape(kv.Key)}\",{kv.Value}");
            }
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string Escape(string input) => input?.Replace("\"", "\"\"") ?? string.Empty;
}
