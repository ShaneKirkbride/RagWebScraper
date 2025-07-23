using RagWebScraper.Models;
using RagWebScraper.Services;

public class AppStateService
{
    public List<AnalysisResult> UrlAnalysisResults { get; private set; } = new();

    public List<AnalysisResult> PdfAnalysisResults { get; private set; } = new();
    public List<AnalysisResult> JsonAnalysisResults { get; private set; } = new();
    public List<EntityGraph> EntityGraphs { get; private set; } = new();

    public string PdfKeywordSummary { get; private set; } = string.Empty;

    public string UrlKeywordSummary { get; private set; } = string.Empty;

    public Dictionary<string, EntityGraph> GraphsByDocument { get; private set; } = new();

    public event Action? OnChange;

    public List<LinkedPassage> PdfCrossDocLinks { get; private set; } = [];
    public List<LinkedPassage> UrlCrossDocLinks { get; private set; } = [];
    public List<LinkedPassage> AllCrossDocLinks { get; private set; } = [];

    public void SetEntityGraphs(IEnumerable<EntityGraph> graphs)
    {
        var list = graphs.ToList();
        EntityGraphs = list;
        GraphsByDocument = list.ToDictionary(g => g.SourceId);
        NotifyStateChanged();
    }

    public void SetEntityGraphsByDocument(IEnumerable<EntityGraph> graphs)
    {
        GraphsByDocument = graphs.ToDictionary(g => g.SourceId);
        NotifyStateChanged();
    }

    public void SetAllEntityGraphs(List<EntityGraph> graphs)
    {
        EntityGraphs = graphs;
        NotifyStateChanged();
    }

    public void SetPdfCrossDocLinks(List<LinkedPassage> links)
    {
        PdfCrossDocLinks = links;
        NotifyStateChanged();
    }

    public void SetUrlCrossDocLinks(List<LinkedPassage> links)
    {
        UrlCrossDocLinks = links;
        NotifyStateChanged();
    }

    public void SetAllCrossDocLinks(List<LinkedPassage> links)
    {
        AllCrossDocLinks = links;
        NotifyStateChanged();
    }

    public void SetUrlResults(List<AnalysisResult> results)
    {
        UrlAnalysisResults = results;
        NotifyStateChanged();
    }

    public void SetPdfResults(List<AnalysisResult> results)
    {
        PdfAnalysisResults = results;
        NotifyStateChanged();
    }

    public void SetJsonResults(List<AnalysisResult> results)
    {
        JsonAnalysisResults = results;
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
