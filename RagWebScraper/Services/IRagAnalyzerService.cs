using RagWebScraper.Models;

namespace RagWebScraper.Services
{
    public interface IRagAnalyzerService
    {
        Task<RagAnalysisResult> AnalyzeAsync(DocumentSet set);
    }
}
