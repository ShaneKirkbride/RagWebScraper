using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System.IO;
using System.Threading.Tasks;
using RagWebScraper.Models;
using RagWebScraper.Services;
using Xunit;

namespace RagWebScraper.Tests;

public class BertTopicClusteringPageTests
{
    private class StubClusterer : IDocumentClusterer
    {
        public List<Document>? ReceivedDocs { get; private set; }
        public int ReceivedK { get; private set; }
        public DocumentClusteringResult Result { get; set; } =
            new(new Dictionary<Guid, int>(), new ClusterMetrics(0, 0, 0), new List<ClusterDescriptor>());
        public Exception? ExceptionToThrow { get; set; }

        public Task<DocumentClusteringResult> ClusterAsync(IEnumerable<Document> documents, int numberOfClusters = 5)
        {
            ReceivedDocs = documents.ToList();
            ReceivedK = numberOfClusters;
            if (ExceptionToThrow != null)
                throw ExceptionToThrow;
            return Task.FromResult(Result);
        }
    }

    private class StubBrowserFile : IBrowserFile
    {
        private readonly Stream _stream;

        public StubBrowserFile(string name, string content)
        {
            Name = name;
            _stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        }

        public DateTimeOffset LastModified => DateTimeOffset.Now;
        public string Name { get; }
        public long Size => _stream.Length;
        public string ContentType => Name.EndsWith(".pdf") ? "application/pdf" : "text/plain";

        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
        {
            _stream.Position = 0;
            return _stream;
        }
    }

    private class StubTextExtractor : ITextExtractor
    {
        public Task<string> ExtractTextAsync(Stream pdfStream)
        {
            using var reader = new StreamReader(pdfStream, leaveOpen: true);
            return Task.FromResult(reader.ReadToEnd());
        }
    }

    private static object GetPrivateField(object obj, string name)
        => obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(obj)!;

    private static void SetPrivateField(object obj, string name, object? value)
        => obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(obj, value);

    private static Task InvokePrivateMethod(object obj, string name)
    {
        var method = obj.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (Task)method.Invoke(obj, Array.Empty<object>())!;
    }

    [Fact]
    public async Task ClusterDocs_ReadsFilesAndCallsClusterer()
    {
        var clusterer = new StubClusterer();
        var page = new RagWebScraper.Pages.BertTopicClustering();
        page.GetType().GetProperty("Clusterer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(page, clusterer);
        page.GetType().GetProperty("AppState", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(page, new AppStateService());
        page.GetType().GetProperty("TextExtractor", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(page, new StubTextExtractor());

        var files = new List<IBrowserFile>
        {
            new StubBrowserFile("a.txt", "Alpha"),
            new StubBrowserFile("b.pdf", "Beta")
        };
        SetPrivateField(page, "selectedFiles", files);
        SetPrivateField(page, "clusterCount", 2);

        await InvokePrivateMethod(page, "ClusterDocs");

        Assert.Equal(2, clusterer.ReceivedDocs!.Count);
        Assert.Equal("Alpha", clusterer.ReceivedDocs[0].Text);
        Assert.Equal("Beta", clusterer.ReceivedDocs[1].Text);
        Assert.Equal(2, clusterer.ReceivedK);
        var pageResult = (DocumentClusteringResult)GetPrivateField(page, "clusterResult");
        Assert.Same(clusterer.Result, pageResult);
    }

    [Fact]
    public async Task ClusterDocs_NoDocumentsClearsResults()
    {
        var clusterer = new StubClusterer
        {
            Result = new DocumentClusteringResult(new() { { Guid.NewGuid(), 1 } }, new ClusterMetrics(0, 0, 0), new List<ClusterDescriptor>())
        };
        var page = new RagWebScraper.Pages.BertTopicClustering();
        page.GetType().GetProperty("Clusterer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(page, clusterer);
        page.GetType().GetProperty("AppState", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(page, new AppStateService());
        page.GetType().GetProperty("TextExtractor", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(page, new StubTextExtractor());

        SetPrivateField(page, "selectedFiles", new List<IBrowserFile>());
        await InvokePrivateMethod(page, "ClusterDocs");

        Assert.Null(GetPrivateField(page, "clusterResult"));
    }

    [Fact]
    public async Task ClusterDocs_TooFewDocsShowsErrorMessage()
    {
        var clusterer = new StubClusterer
        {
            ExceptionToThrow = new InvalidOperationException("too few")
        };
        var page = new RagWebScraper.Pages.BertTopicClustering();
        page.GetType().GetProperty("Clusterer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(page, clusterer);
        page.GetType().GetProperty("AppState", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(page, new AppStateService());
        page.GetType().GetProperty("TextExtractor", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(page, new StubTextExtractor());

        var files = new List<IBrowserFile> { new StubBrowserFile("a.txt", "Alpha") };
        SetPrivateField(page, "selectedFiles", files);
        SetPrivateField(page, "clusterCount", 3);

        await InvokePrivateMethod(page, "ClusterDocs");

        Assert.Null(GetPrivateField(page, "clusterResult"));
        Assert.Equal("too few", GetPrivateField(page, "errorMessage"));
    }

    [Fact]
    public void BuildRenderTree_IgnoresMissingLabels()
    {
        var page = new RagWebScraper.Pages.BertTopicClustering();
        var results = new Dictionary<Guid, int> { [Guid.NewGuid()] = 0 };
        var clusteringResult = new DocumentClusteringResult(results, new ClusterMetrics(0, 0, 0), new List<ClusterDescriptor>());
        SetPrivateField(page, "clusterResult", clusteringResult);
        var builder = new RenderTreeBuilder();
        var method = page.GetType().GetMethod("BuildRenderTree", BindingFlags.Instance | BindingFlags.NonPublic)!;

        var ex = Record.Exception(() => method.Invoke(page, new object[] { builder }));

        Assert.Null(ex);
    }
}
