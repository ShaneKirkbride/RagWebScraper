using Xunit;
using RagWebScraper.Services;

namespace RagWebScraper.Tests;

public class TextChunkerTests
{
    [Fact]
    public void ChunkText_ReturnsEmptyList_ForNullOrWhitespace()
    {
        // Arrange
        var chunker = new TextChunker();

        // Act
        var resultNull = chunker.ChunkText(null!);
        var resultWhitespace = chunker.ChunkText(" \t\n");

        // Assert
        Assert.Empty(resultNull);
        Assert.Empty(resultWhitespace);
    }

    [Fact]
    public void ChunkText_SplitsByTokenLimit()
    {
        // Arrange
        var chunker = new TextChunker();
        var text = "Sentence one. Sentence two is longer. Sentence three.";

        // Act
        var chunks = chunker.ChunkText(text, maxTokensPerChunk: 3);

        // Assert
        Assert.Equal(3, chunks.Count);
        Assert.Contains("Sentence one.", chunks[0]);
        Assert.Contains("Sentence two is longer.", chunks[1]);
        Assert.Contains("Sentence three.", chunks[2]);
    }

    [Fact]
    public void ChunkText_LongSentence_RemainsSingleChunk()
    {
        // Arrange
        var chunker = new TextChunker();
        var text = "One two three four five six seven. Short.";

        // Act
        var chunks = chunker.ChunkText(text, maxTokensPerChunk: 3);

        // Assert
        Assert.Equal(2, chunks.Count);
        Assert.Contains("seven.", chunks[0]);
    }

    [Fact]
    public void ChunkText_RespectsTokenLimit()
    {
        // Arrange
        var chunker = new TextChunker();
        var text = "A B. C D. E F.";

        // Act
        var chunks = chunker.ChunkText(text, maxTokensPerChunk: 2);

        // Assert
        Assert.All(chunks, c => Assert.True(c.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length <= 2));
    }
}
