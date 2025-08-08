using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using RagWebScraper.Models;
using RagWebScraper.Services;
using Xunit;

namespace RagWebScraper.Tests;

public class PdfProcessingWorkerTests
{
    private class StubExtractor : ITextExtractor
    {
        public Task<string> ExtractTextAsync(Stream pdfStream) => Task.FromResult("text");
    }

    private class StubSentiment : ISentimentAnalyzer
    {
        public float AnalyzeSentiment(string text) => 0.5f;
    }

    private class StubKeywordExtractor : IKeywordExtractor
    {
        public Dictionary<string, int> ExtractKeywords(string text, List<string> keywords) => new();
    }

    private class StubKeywordContext : IKeywordContextSentimentService
    {
        public Dictionary<string, float> ExtractKeywordSentiments(string text, IEnumerable<string> keywords) => new();
    }

    private class StubChunkIngestor : IChunkIngestorService
    {
        public Task IngestChunksAsync(string sourceLabel, string text, Dictionary<string, object>? extraMetadata = null) => Task.CompletedTask;
    }

    private class StubQueue : IPdfProcessingQueue
    {
        public void Enqueue(PdfProcessingRequest request) { }
        public async IAsyncEnumerable<PdfProcessingRequest> ReadAllAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield break;
        }
    }

    private class TestWorker : PdfProcessingWorker
    {
        public TestWorker() : base(new StubQueue(), NullLogger<PdfProcessingWorker>.Instance, new StubExtractor(), new StubSentiment(), new StubKeywordExtractor(), new StubKeywordContext(), new StubChunkIngestor(), new PdfResultStore()) { }
        public Task HandleAsync(PdfProcessingRequest request) => base.ProcessRequestAsync(request, CancellationToken.None);
    }

    [Fact]
    public async Task ProcessRequestAsync_DeletesFileAfterProcessing()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pdf");
        await File.WriteAllTextAsync(path, "dummy");
        var worker = new TestWorker();

        await worker.HandleAsync(new PdfProcessingRequest
        {
            FileName = "test.pdf",
            FilePath = path,
            Keywords = new List<string>()
        });

        Assert.False(File.Exists(path));
    }
}
