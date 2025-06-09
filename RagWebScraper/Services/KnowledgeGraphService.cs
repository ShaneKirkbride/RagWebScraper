using RagWebScraper.Models;
using RagWebScraper.Services;

public class KnowledgeGraphService : IKnowledgeGraphService
{
    private readonly IEntityGraphExtractor _graphExtractor;
    private readonly PdfResultStore _resultStore;

    public KnowledgeGraphService(IEntityGraphExtractor graphExtractor, PdfResultStore resultStore)
    {
        _graphExtractor = graphExtractor;
        _resultStore = resultStore;
    }

    public Task<EntityGraphResult> AnalyzeTextAsync(string text)
    {
        var graph = _graphExtractor.Extract(text, sourceId: "manual-input");

        return Task.FromResult(new EntityGraphResult
        {
            Nodes = graph.Nodes,
            Edges = graph.Edges
        });
    }
    public Task<EntityGraphResult> AnalyzePdfAsync(string fileName)
    {
        if (!_resultStore.TryGet(fileName, out var result) || string.IsNullOrWhiteSpace(result.RawText))
            throw new InvalidOperationException($"No processed PDF found for '{fileName}'.");

        var graph = _graphExtractor.Extract(result.RawText, fileName);

        return Task.FromResult(new EntityGraphResult
        {
            Nodes = graph.Nodes,
            Edges = graph.Edges
        });
    }
}
