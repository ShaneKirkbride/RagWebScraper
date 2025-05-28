using RagWebScraper.Models;

namespace RagWebScraper.Services
{
    public interface IBiasAnalyzer
    {
        BiasAnalysisResult Analyze(string text, string context = null);
    }
}
