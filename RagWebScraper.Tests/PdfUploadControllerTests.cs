using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RagWebScraper.Controllers;
using RagWebScraper.Models;
using RagWebScraper.Services;
using Xunit;

namespace RagWebScraper.Tests;

public class PdfUploadControllerTests
{
    private class StubTextExtractor : ITextExtractor
    {
        public Task<string> ExtractTextAsync(Stream pdfStream) => Task.FromResult(string.Empty);
    }

    private class StubSentimentAnalyzer : ISentimentAnalyzer
    {
        public float AnalyzeSentiment(string text) => 0f;
    }

    private class StubEmbeddingService : IEmbeddingService
    {
        public Task<List<float>> GetEmbeddingAsync(string input) => Task.FromResult(new List<float>());
        public Task<List<float[]>> GetEmbeddingsAsync(IEnumerable<string> inputs) => Task.FromResult(new List<float[]>());
    }

    private class StubVectorStore : IVectorStoreService
    {
        public Task UpsertVectorAsync(VectorData vectorData) => Task.CompletedTask;
        public Task<List<QdrantResult>> QueryAsync(List<float> embedding, int topK = 3) => Task.FromResult(new List<QdrantResult>());
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
        public List<PdfProcessingRequest> Items { get; } = new();
        public void Enqueue(PdfProcessingRequest request) => Items.Add(request);
        public async IAsyncEnumerable<PdfProcessingRequest> ReadAllAsync(CancellationToken cancellationToken)
        {
            foreach (var item in Items)
            {
                yield return item;
                await Task.CompletedTask;
            }
        }
    }

    private static IFormFile CreateFormFile(long size, string name = "file.pdf")
    {
        var stream = new MemoryStream(new byte[size]);
        return new FormFile(stream, 0, size, "files", name)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };
    }

    private static PdfUploadController CreateController(FileUploadOptions options, StubQueue queue)
    {
        return new PdfUploadController(
            new StubTextExtractor(),
            new StubSentimentAnalyzer(),
            new TextChunker(),
            new StubEmbeddingService(),
            new StubVectorStore(),
            new StubKeywordExtractor(),
            new StubKeywordContext(),
            new StubChunkIngestor(),
            queue,
            Options.Create(options));
    }

    [Fact]
    public async Task AnalyzePdf_RejectsFileOverLimit()
    {
        var options = new FileUploadOptions { MaxFileSize = 10, MaxRequestSize = 100 };
        var queue = new StubQueue();
        var controller = CreateController(options, queue);

        var files = new FormFileCollection { CreateFormFile(20, "big.pdf") };

        var result = await controller.AnalyzePdf(files, "");

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(queue.Items);
    }

    [Fact]
    public async Task AnalyzePdf_RejectsWhenTotalTooLarge()
    {
        var options = new FileUploadOptions { MaxFileSize = 50, MaxRequestSize = 80 };
        var queue = new StubQueue();
        var controller = CreateController(options, queue);

        var files = new FormFileCollection
        {
            CreateFormFile(40, "a.pdf"),
            CreateFormFile(45, "b.pdf")
        };

        var result = await controller.AnalyzePdf(files, "");

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(queue.Items);
    }

    [Fact]
    public async Task AnalyzePdf_EnqueuesFiles_WhenWithinLimits()
    {
        var options = new FileUploadOptions { MaxFileSize = 50, MaxRequestSize = 150 };
        var queue = new StubQueue();
        var controller = CreateController(options, queue);

        var files = new FormFileCollection
        {
            CreateFormFile(40, "a.pdf"),
            CreateFormFile(30, "b.pdf")
        };

        var result = await controller.AnalyzePdf(files, "alpha,beta");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(2, queue.Items.Count);
        Assert.All(queue.Items, r => Assert.Contains(r.FileName, new[] { "a.pdf", "b.pdf" }));
    }

    [Fact]
    public async Task AnalyzePdf_AllowsManyFiles()
    {
        var options = new FileUploadOptions { MaxFileSize = 10, MaxRequestSize = 10_000 };
        var queue = new StubQueue();
        var controller = CreateController(options, queue);

        var files = new FormFileCollection();
        for (var i = 0; i < 300; i++)
        {
            files.Add(CreateFormFile(1, $"file{i}.pdf"));
        }

        var result = await controller.AnalyzePdf(files, "");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(300, queue.Items.Count);
    }
}
