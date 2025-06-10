using RagWebScraper.Services;

namespace RagWebScraper.Tests;

public class TextChunkerTests
{
    [Fact]
    public void ChunkText_ReturnsEmptyList_ForNullOrWhitespace()
    {
        var chunker = new TextChunker();
        var result = chunker.ChunkText(null!);
        Assert.Empty(result);
        result = chunker.ChunkText(" \t\n");
        Assert.Empty(result);
    }

    [Fact]
    public void ChunkText_SplitsByTokenLimit()
    {
        var chunker = new TextChunker();
        var text = "Sentence one. Sentence two is longer. Sentence three.";
        var chunks = chunker.ChunkText(text, maxTokensPerChunk: 3);
        Assert.Equal(3, chunks.Count);
        Assert.Contains("Sentence one.", chunks[0]);
        Assert.Contains("Sentence two is longer.", chunks[1]);
        Assert.Contains("Sentence three.", chunks[2]);
    }
}
