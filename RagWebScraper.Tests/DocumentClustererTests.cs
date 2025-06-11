using RagWebScraper.Models;
using RagWebScraper.Services;
using Xunit;

namespace RagWebScraper.Tests;

public class DocumentClustererTests
{
    [Fact]
    public async Task ClusterAsync_ReturnsClusterAssignments()
    {
        var docs = new[]
        {
            new Document(Guid.NewGuid(), "Cats are wonderful pets"),
            new Document(Guid.NewGuid(), "Dogs are loyal companions"),
            new Document(Guid.NewGuid(), "I love my cat"),
            new Document(Guid.NewGuid(), "Walking the dog is fun")
        };

        IDocumentClusterer clusterer = new TfidfKMeansClusterer();
        var result = await clusterer.ClusterAsync(docs, numberOfClusters: 2);

        Assert.Equal(docs.Length, result.Count);
        var unique = new HashSet<int>(result.Values);
        Assert.Equal(2, unique.Count);
    }
}
