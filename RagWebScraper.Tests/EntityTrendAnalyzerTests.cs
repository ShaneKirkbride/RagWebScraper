using System;
using System.Collections.Generic;
using System.Linq;
using RagWebScraper.Models;
using RagWebScraper.Services;
using Xunit;

namespace RagWebScraper.Tests;

public class EntityTrendAnalyzerTests
{
    [Fact]
    public void ComputeTrends_GroupsByMonth()
    {
        var docs = new[]
        {
            new DocumentAnalysisResult(new DateTime(2023,1,1), new []
            {
                new Entity("ORG","Microsoft",0,1),
                new Entity("ORG","OpenAI",2,3)
            }),
            new DocumentAnalysisResult(new DateTime(2023,1,5), new []
            {
                new Entity("ORG","Microsoft",0,1),
                new Entity("ORG","Amazon",2,3)
            }),
            new DocumentAnalysisResult(new DateTime(2023,2,15), new []
            {
                new Entity("ORG","Google",0,1)
            })
        };

        ITrendAnalyzer analyzer = new EntityTrendAnalyzer();
        var trends = analyzer.ComputeTrends(docs, TimeSpan.FromDays(30)).ToList();

        Assert.Equal(4, trends.Count);
        Assert.Equal(2, trends.Single(t => t.Entity == "Microsoft" && t.Period == "2023-01").Count);
        Assert.Equal(1, trends.Single(t => t.Entity == "OpenAI" && t.Period == "2023-01").Count);
        Assert.Equal(1, trends.Single(t => t.Entity == "Amazon" && t.Period == "2023-01").Count);
        Assert.Equal(1, trends.Single(t => t.Entity == "Google" && t.Period == "2023-02").Count);
    }
}
