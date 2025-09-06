using RagWebScraper.Models;
using RagWebScraper.Services;
using Xunit;

namespace RagWebScraper.Tests;

public class BertTopicClustererTests
{
    [Fact]
    public async Task ClusterAsync_ReturnsAssignmentsAndDescriptors()
    {
        var docs = new[]
        {
            new Document(Guid.NewGuid(), "Cats are wonderful pets"),
            new Document(Guid.NewGuid(), "Dogs are loyal companions"),
            new Document(Guid.NewGuid(), "I love my cat"),
            new Document(Guid.NewGuid(), "Walking the dog is fun")
        };

        IDocumentClusterer clusterer = new BertTopicClusterer();
        var result = await clusterer.ClusterAsync(docs, numberOfClusters: 2);

        Assert.Equal(docs.Length, result.Clusters.Count);
        Assert.True(result.Descriptors.Count > 0);
    }
}
