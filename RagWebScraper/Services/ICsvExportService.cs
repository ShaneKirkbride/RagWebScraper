using RagWebScraper.Models;

namespace RagWebScraper.Services;

public interface ICsvExportService
{
    byte[] ExportPageSentiment(IEnumerable<AnalysisResult> results);
    byte[] ExportCrossLinks(IEnumerable<LinkedPassage> links);
    byte[] ExportKeywordSentiment(IEnumerable<AnalysisResult> results);
    byte[] ExportKeywordFrequencies(IEnumerable<AnalysisResult> results);
}
