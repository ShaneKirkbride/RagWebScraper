using Xunit;
using RagWebScraper.Models;
using RagWebScraper.Services;
using System.Collections.Generic;

namespace RagWebScraper.Tests;

public class KnowledgeGraphServiceTests
{
    private class StubGraphExtractor : IEntityGraphExtractor
    {
        public EntityGraph Extract(string text, string sourceId)
            => new EntityGraph(sourceId, new List<EntityNode>(), new List<EntityEdge>());
    }

    private class StubNerService : INerService
    {
        private readonly List<(string Token, string Label)> _tokens;

        public StubNerService(List<(string Token, string Label)> tokens)
        {
            _tokens = tokens;
        }

        public List<(string Token, string Label)> RecognizeTokensWithLabels(string sentence)
            => _tokens;

        public List<NamedEntity> RecognizeEntities(string text) => new();
    }

    [Fact]
    public async Task AnalyzeTextAsync_ReturnsTaggedSentences()
    {
        var tokens = new List<(string Token, string Label)>
        {
            ("Alice", "B-PER"),
            ("works", "O"),
            ("at", "O"),
            ("Contoso", "B-ORG")
        };

        var service = new KnowledgeGraphService(new StubGraphExtractor(), new PdfResultStore(), new StubNerService(tokens));

        var result = await service.AnalyzeTextAsync("Alice works at Contoso");

        Assert.Single(result.LabeledSentences);
        Assert.Equal("<B-PER>Alice</B-PER> <O>works</O> <O>at</O> <B-ORG>Contoso</B-ORG>", result.LabeledSentences[0]);
    }

    [Fact]
    public async Task AnalyzePdfAsync_UsesStoredText()
    {
        var tokens = new List<(string Token, string Label)>
        {
            ("Alice", "B-PER"),
            ("works", "O"),
            ("at", "O"),
            ("Contoso", "B-ORG")
        };

        var store = new PdfResultStore();
        var analysis = new AnalysisResult(new List<LinkedPassage>()) { FileName = "file.pdf" };
        typeof(AnalysisResult).GetProperty("RawText")!.SetValue(analysis, "Alice works at Contoso");
        store.Add(analysis);

        var service = new KnowledgeGraphService(new StubGraphExtractor(), store, new StubNerService(tokens));

        var result = await service.AnalyzePdfAsync("file.pdf");

        Assert.Single(result.LabeledSentences);
        Assert.Equal("<B-PER>Alice</B-PER> <O>works</O> <O>at</O> <B-ORG>Contoso</B-ORG>", result.LabeledSentences[0]);
    }
}
