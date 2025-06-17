using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using RagWebScraper.Models;
using RagWebScraper.Services;
using Xunit;

namespace RagWebScraper.Tests;

public class FileCourtListenerServiceTests
{
    [Fact]
    public async Task GetOpinionsAsync_ReadsOpinionsFromFile()
    {
        var data = new
        {
            results = new[]
            {
                new { id = 1, case_name = "a", plain_text = "A" },
                new { id = 2, case_name = "b", plain_text = "B" }
            }
        };

        var file = Path.GetTempFileName();
        await File.WriteAllTextAsync(file, JsonSerializer.Serialize(data));

        var service = new FileCourtListenerService();
        var opinions = new List<CourtOpinion>();
        await foreach (var op in service.GetOpinionsAsync(file))
        {
            opinions.Add(op);
        }

        File.Delete(file);

        Assert.Equal(2, opinions.Count);
        Assert.Equal("a", opinions[0].CaseName);
        Assert.Equal("B", opinions[1].PlainText);
    }
}
