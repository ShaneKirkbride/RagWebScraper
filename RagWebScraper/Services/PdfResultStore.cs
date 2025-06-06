using RagWebScraper.Models;
using RagWebScraper.Services;
using System.Collections.Concurrent;

public class PdfResultStore
{
    private readonly ConcurrentDictionary<string, AnalysisResult> _results = new();

    public void Add(AnalysisResult summary)
    {
        _results[summary.FileName] = summary;
    }

    public List<AnalysisResult> GetAll() => _results.Values.ToList();
}
