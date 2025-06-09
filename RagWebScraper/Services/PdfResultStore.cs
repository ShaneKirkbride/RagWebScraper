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

    public bool TryGet(string fileName, out AnalysisResult? result)
    {
        return _results.TryGetValue(fileName, out result);
    }

    public bool Contains(string fileName) => _results.ContainsKey(fileName);

    public bool Remove(string fileName) => _results.TryRemove(fileName, out _);
}

