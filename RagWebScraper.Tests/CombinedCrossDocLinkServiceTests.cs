using RagWebScraper.Models;
using RagWebScraper.Services;
using Xunit;

namespace RagWebScraper.Tests;

public class CombinedCrossDocLinkServiceTests
{
    private class StubAnalyzer : IRagAnalyzerService
    {
        private readonly RagAnalysisResult _result;
        public DocumentSet? ReceivedSet { get; private set; }
        public StubAnalyzer(RagAnalysisResult result)
        {
            _result = result;
        }
        public Task<RagAnalysisResult> AnalyzeAsync(DocumentSet set)
        {
            ReceivedSet = set;
            return Task.FromResult(_result);
        }
    }

    [Fact]
    public async Task ComputeLinksAsync_ReturnsLinksAcrossAllDocs()
    {
        var expectedLinks = new List<LinkedPassage>
        {
            new("a","textA","b","textB",0.9f)
        };
        var analyzer = new StubAnalyzer(new RagAnalysisResult(expectedLinks, []));
        var service = new CombinedCrossDocLinkService(analyzer, new TextChunker());

        var urlResults = new List<AnalysisResult>
        {
            new([]){ Url = "a" },
            new([]){ Url = "b" }
        };
        typeof(AnalysisResult).GetProperty("RawText")!.SetValue(urlResults[0], "Sentence one.");
        typeof(AnalysisResult).GetProperty("RawText")!.SetValue(urlResults[1], "Sentence two.");
        var pdfResults = new List<AnalysisResult>
        {
            new([]){ FileName = "c.pdf" }
        };
        typeof(AnalysisResult).GetProperty("RawText")!.SetValue(pdfResults[0], "Sentence three.");

        var links = await service.ComputeLinksAsync(urlResults, pdfResults);

        Assert.Equal(expectedLinks, links);
        Assert.Equal(3, analyzer.ReceivedSet?.Documents.Count);
    }

    [Fact]
    public async Task ComputeLinksAsync_ReturnsEmpty_WhenSingleDoc()
    {
        var analyzer = new StubAnalyzer(new RagAnalysisResult(new List<LinkedPassage>(), []));
        var service = new CombinedCrossDocLinkService(analyzer, new TextChunker());

        var urlResults = new List<AnalysisResult>
        {
            new([]){ Url = "a" }
        };
        typeof(AnalysisResult).GetProperty("RawText")!.SetValue(urlResults[0], "Only one.");

        var links = await service.ComputeLinksAsync(urlResults, new List<AnalysisResult>());

        Assert.Empty(links);
        Assert.Null(analyzer.ReceivedSet);
    }
}
