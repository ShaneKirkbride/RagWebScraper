// Services/AppStateService.cs
using RagWebScraper.Models;
using RagWebScraper.Services;
using static RagWebScraper.Pages.UploadPdf;

public class AppStateService
{
    public List<AnalysisResult> UrlAnalysisResults { get; private set; } = new();

    public List<FileSentimentSummary> PdfAnalysisResults { get; private set; } = new();

    public string PdfKeywordSummary { get; private set; } = string.Empty;

    public string UrlKeywordSummary { get; private set; } = string.Empty;

    public event Action? OnChange;

    public void SetUrlResults(List<AnalysisResult> results)
    {
        UrlAnalysisResults = results;
        NotifyStateChanged();
    }

    public void SetPdfResults(List<FileSentimentSummary> results)
    {
        PdfAnalysisResults = results;
        NotifyStateChanged();
    }

    public void SetPdfKeywordSummary(string summary)
    {
        PdfKeywordSummary = summary;
        NotifyStateChanged();
    }
       
    public void SetUrlKeywordSummary(string summary)
    {
        UrlKeywordSummary = summary;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
