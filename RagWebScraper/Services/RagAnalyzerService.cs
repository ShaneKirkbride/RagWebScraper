using RagWebScraper.Models;

namespace RagWebScraper.Services;

public sealed class RagAnalyzerService : IRagAnalyzerService
{
    private readonly ICrossDocumentLinker _linker;
    private readonly IEntityGraphExtractor _entityGraphExtractor;

    public RagAnalyzerService(ICrossDocumentLinker linker, IEntityGraphExtractor graphExtractor)
    {
        _linker = linker;
        _entityGraphExtractor = graphExtractor;
    }

    public async Task<RagAnalysisResult> AnalyzeAsync(DocumentSet set)
    {
        var chunks = set.Documents.SelectMany(d => d.Chunks).ToList();
        var links = await _linker.LinkAsync(chunks);

        var graphs = set.Documents
            .Select(d => _entityGraphExtractor.Extract(
                string.Join(" ", d.Chunks.Select(c => c.Text)), d.SourceId))
            .ToList();

        return new RagAnalysisResult(links, graphs);
    }
}
