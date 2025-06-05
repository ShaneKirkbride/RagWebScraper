namespace RagWebScraper.Services
{
    public interface IAnalysisService
    {
        Task AnalyzePdfAsync(string pdfFilePath);
    }
}