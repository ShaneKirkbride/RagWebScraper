using RagWebScraper.Services;
using System.Collections.Concurrent;

public class PdfResultStore
{
    private readonly ConcurrentDictionary<string, FileSentimentSummary> _results = new();

    public void Add(FileSentimentSummary summary)
    {
        _results[summary.FileName] = summary;
    }

    public List<FileSentimentSummary> GetAll() => _results.Values.ToList();
}
