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

    [Fact]
    public async Task ClusterAsync_DoesNotThrowSchemaMismatch()
    {
        var docs = new[]
        {
            new Document(Guid.NewGuid(), new string('a', 50)),
            new Document(Guid.NewGuid(), new string('b', 100)),
            new Document(Guid.NewGuid(), new string('c', 150))
        };

        IDocumentClusterer clusterer = new TfidfKMeansClusterer();

        var ex = await Record.ExceptionAsync(() => clusterer.ClusterAsync(docs, 2));

        Assert.Null(ex);
    }

    [Fact]
    public async Task ClusterAsync_HandlesWhitespaceAndLongDocs()
    {
        var docs = new[]
        {
            new Document(Guid.NewGuid(), string.Empty),
            new Document(Guid.NewGuid(), "   \t"),
            new Document(Guid.NewGuid(), "Example text with punctuation!"),
            new Document(Guid.NewGuid(), new string('x', 500))
        };

        IDocumentClusterer clusterer = new TfidfKMeansClusterer();

        var ex = await Record.ExceptionAsync(() => clusterer.ClusterAsync(docs, 2));

        Assert.Null(ex);
    }
}
