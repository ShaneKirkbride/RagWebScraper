using System;
using Xunit;
using RagWebScraper.Services;

namespace RagWebScraper.Tests;

public class TextChunkerTests
{
    [Fact]
    public void ChunkText_SplitsIntoExpectedChunks()
    {
        var text = string.Join(" ", new[]
        {
            "One two three.",
            "Four five.",
            "Six seven eight.",
            "Nine.",
            "Ten eleven twelve thirteen.",
            "Fourteen."
        });

        var chunker = new TextChunker();
        var chunks = chunker.ChunkText(text, maxTokensPerChunk: 6);

        Assert.Equal(3, chunks.Count);
        Assert.Equal(5, CountTokens(chunks[0]));
        Assert.Equal(4, CountTokens(chunks[1]));
        Assert.Equal(5, CountTokens(chunks[2]));
    }

    private static int CountTokens(string text)
        => text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
}
