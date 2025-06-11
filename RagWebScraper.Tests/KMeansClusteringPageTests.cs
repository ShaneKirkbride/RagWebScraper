using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RagWebScraper.Models;
using RagWebScraper.Services;
using Xunit;

namespace RagWebScraper.Tests;

public class KMeansClusteringPageTests
{
    private class StubClusterer : IDocumentClusterer
    {
        public List<Document>? ReceivedDocs { get; private set; }
        public int ReceivedK { get; private set; }
        public Dictionary<Guid, int> Result { get; set; } = new();

        public Task<Dictionary<Guid, int>> ClusterAsync(IEnumerable<Document> documents, int numberOfClusters = 5)
        {
            ReceivedDocs = documents.ToList();
            ReceivedK = numberOfClusters;
            return Task.FromResult(Result);
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
    public async Task ClusterDocs_ParsesInputAndCallsClusterer()
    {
        var clusterer = new StubClusterer();
        var page = new RagWebScraper.Pages.KMeansClustering();
        page.GetType().GetProperty("Clusterer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(page, clusterer);

        SetPrivateField(page, "documentsInput", "A\nB");
        SetPrivateField(page, "clusterCount", 2);

        await InvokePrivateMethod(page, "ClusterDocs");

        Assert.Equal(2, clusterer.ReceivedDocs!.Count);
        Assert.Equal(2, clusterer.ReceivedK);
        Assert.Same(clusterer.Result, GetPrivateField(page, "clusterResults"));
    }

    [Fact]
    public async Task ClusterDocs_NoDocumentsClearsResults()
    {
        var clusterer = new StubClusterer { Result = new() { { Guid.NewGuid(), 1 } } };
        var page = new RagWebScraper.Pages.KMeansClustering();
        page.GetType().GetProperty("Clusterer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(page, clusterer);

        SetPrivateField(page, "documentsInput", string.Empty);
        await InvokePrivateMethod(page, "ClusterDocs");

        Assert.Null(GetPrivateField(page, "clusterResults"));
    }
}
