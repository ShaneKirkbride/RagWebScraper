using RagWebScraper.Models;
using RagWebScraper.Services;

public class KnowledgeGraphService : IKnowledgeGraphService
{
    private readonly IEntityGraphExtractor _graphExtractor;
    private readonly PdfResultStore _resultStore;
    private readonly INerService _nerService;

    public KnowledgeGraphService(IEntityGraphExtractor graphExtractor, PdfResultStore resultStore, INerService nerService)
    {
        _graphExtractor = graphExtractor;
        _resultStore = resultStore;
        _nerService = nerService;
    }

    public Task<EntityGraphResult> AnalyzeTextAsync(string text)
    {
        var graph = _graphExtractor.Extract(text, sourceId: "manual-input");
        var labeled = BuildLabeledSentences(text);

        return Task.FromResult(new EntityGraphResult
        {
            Nodes = graph.Nodes,
            Edges = graph.Edges,
            LabeledSentences = labeled
        });
    }

    public Task<EntityGraphResult> AnalyzePdfAsync(string fileName)
    {
        if (!_resultStore.TryGet(fileName, out var result) || string.IsNullOrWhiteSpace(result.RawText))
            throw new InvalidOperationException($"No processed PDF found for '{fileName}'.");

        var graph = _graphExtractor.Extract(result.RawText, fileName);
        var labeled = BuildLabeledSentences(result.RawText);

        return Task.FromResult(new EntityGraphResult
        {
            Nodes = graph.Nodes,
            Edges = graph.Edges,
            LabeledSentences = labeled
        });
    }

    private List<string> BuildLabeledSentences(string text)
    {
        var sentences = SentenceSplitter.Split(text);
        var labeled = new List<string>(sentences.Count);

        foreach (var sentence in sentences)
        {
            var tokens = _nerService.RecognizeTokensWithLabels(sentence);
            var xml = string.Join(" ", tokens.Select(t => $"<{t.Label}>{t.Token}</{t.Label}>"));
            labeled.Add(xml);
        }

        return labeled;
    }
}
