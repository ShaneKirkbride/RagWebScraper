using RagWebScraper.Models;
using System.Linq;

namespace RagWebScraper.Services;

/// <summary>
/// Generates semantic cross-document links across all analyzed sources.
/// </summary>
public class CombinedCrossDocLinkService
{
    private readonly IRagAnalyzerService _analyzer;
    private readonly TextChunker _chunker;

    public CombinedCrossDocLinkService(IRagAnalyzerService analyzer, TextChunker chunker)
    {
        _analyzer = analyzer;
        _chunker = chunker;
    }

    /// <summary>
    /// Computes cross-document links across the provided URL and PDF analysis results.
    /// </summary>
    /// <param name="urlResults">Analyzed URL results.</param>
    /// <param name="pdfResults">Analyzed PDF results.</param>
    public async Task<List<LinkedPassage>> ComputeLinksAsync(IEnumerable<AnalysisResult> urlResults, IEnumerable<AnalysisResult> pdfResults)
    {
        var docs = new List<AnalyzedDocument>();

        foreach (var res in urlResults)
        {
            var chunks = _chunker.ChunkText(res.RawText)
                .Select(t => new DocumentChunk(res.Url, t))
                .ToList();
            docs.Add(new AnalyzedDocument(res.Url, chunks));
        }

        foreach (var res in pdfResults)
        {
            var chunks = _chunker.ChunkText(res.RawText)
                .Select(t => new DocumentChunk(res.FileName, t))
                .ToList();
            docs.Add(new AnalyzedDocument(res.FileName, chunks));
        }

        if (docs.Count <= 1)
            return new List<LinkedPassage>();

        var set = new DocumentSet(docs);
        var analysis = await _analyzer.AnalyzeAsync(set).ConfigureAwait(false);
        return analysis.Links.ToList();
    }
}
