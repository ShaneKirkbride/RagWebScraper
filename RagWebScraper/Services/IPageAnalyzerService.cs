namespace RagWebScraper.Services;

using RagWebScraper.Models;

public interface IPageAnalyzerService
{
    Task<AnalysisResult?> AnalyzePageAsync(string url, List<string> keywords);
}
