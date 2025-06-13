using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using RagWebScraper.Models;
using RagWebScraper.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RagWebScraper.Tests;

public class ONNXNerServiceTests
{
    private class StubTokenizer : ITokenizer
    {
        public (IReadOnlyList<int> Ids, IReadOnlyList<string> Tokens) Encode(string text)
        {
            var ids = Enumerable.Range(0, 520).ToList();
            var tokens = ids.Select(i => $"t{i}").ToList();
            return (ids, tokens);
        }
    }

    private class StubDisposableCollection : List<NamedOnnxValue>, IDisposableReadOnlyCollection<NamedOnnxValue>
    {
        public StubDisposableCollection(IEnumerable<NamedOnnxValue> values) : base(values) { }
        public void Dispose() { }
    }

    private class StubSession : IOnnxSession
    {
        public List<int> RunLengths { get; } = new();

        public IDisposableReadOnlyCollection<NamedOnnxValue> Run(IEnumerable<NamedOnnxValue> inputs)
        {
            var tensor = inputs.First(v => v.Name == "input_ids").AsTensor<long>();
            RunLengths.Add((int)tensor.Length);
            int tokenCount = (int)tensor.Length;
            int labelCount = 9;
            var data = new DenseTensor<float>(new float[tokenCount * labelCount], new[] { 1, tokenCount, labelCount });
            var value = NamedOnnxValue.CreateFromTensor("logits", data);
            return new StubDisposableCollection(new[] { value });
        }

        public void Dispose() { }
    }

    [Fact]
    public void RecognizeTokensWithLabels_UsesWindowsForLongInput()
    {
        var session = new StubSession();
        var service = new ONNXNerService(new StubTokenizer(), session);
        var result = service.RecognizeTokensWithLabels("x");

        Assert.Equal(520, result.Count);
        Assert.Equal(new[] { 512, 8 }, session.RunLengths);
    }
}

