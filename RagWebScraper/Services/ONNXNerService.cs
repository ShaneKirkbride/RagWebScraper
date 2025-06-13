using Microsoft.ML.Tokenizers;
using Microsoft.ML;
using RagWebScraper.Models;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;

namespace RagWebScraper.Services;

public class ONNXNerService : INerService
{
    private readonly ITokenizer _tokenizer;
    private readonly IOnnxSession _session;
    private readonly string[] _labels =
    {
        "O", "B-PER", "I-PER", "B-ORG", "I-ORG", "B-LOC", "I-LOC", "B-MISC", "I-MISC"
    };

    private const int MaxTokens = 512;

    public ONNXNerService(string modelPath, string vocabPath, string mergesPath, string dictionaryPath)
    {
        var tokenizer = EnglishRobertaTokenizer.Create(vocabPath, mergesPath, dictionaryPath);
        _tokenizer = new EnglishRobertaTokenizerAdapter(tokenizer);
        _session = new OnnxSessionWrapper(modelPath);
    }

    public ONNXNerService(ITokenizer tokenizer, IOnnxSession session)
    {
        _tokenizer = tokenizer;
        _session = session;
    }

    private static IEnumerable<(long[] InputIds, List<string> Tokens, int Offset)>
        SplitIntoWindows(IReadOnlyList<int> ids, IReadOnlyList<string> tokens, int overlap = 0)
    {
        if (ids.Count != tokens.Count)
            throw new ArgumentException("Token id and token count mismatch");
        if (overlap < 0 || overlap >= MaxTokens)
            throw new ArgumentOutOfRangeException(nameof(overlap));

        int start = 0;
        while (start < ids.Count)
        {
            var windowIds = ids.Skip(start).Take(MaxTokens).Select(i => (long)i).ToArray();
            var windowTokens = tokens.Skip(start).Take(MaxTokens).ToList();
            yield return (windowIds, windowTokens, start);
            if (start + MaxTokens >= ids.Count)
                break;
            start += MaxTokens - overlap;
        }
    }

    private int[] PredictLabels(long[] inputIds)
    {
        long[] attentionMask = Enumerable.Repeat(1L, inputIds.Length).ToArray();

        var inputIdsTensor = new DenseTensor<long>(inputIds, new[] { 1, inputIds.Length });
        var attentionMaskTensor = new DenseTensor<long>(attentionMask, new[] { 1, attentionMask.Length });

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor),
        };

        using var results = _session.Run(inputs);
        var logits = results.First().AsEnumerable<float>().ToArray();

        int tokenCount = inputIds.Length;
        int labelCount = logits.Length / tokenCount;

        var labelIds = new int[tokenCount];

        for (int i = 0; i < tokenCount; i++)
        {
            int offset = i * labelCount;
            var tokenLogits = logits.Skip(offset).Take(labelCount).ToArray();
            labelIds[i] = Array.IndexOf(tokenLogits, tokenLogits.Max());
        }

        return labelIds;
    }
    
    public List<(string Token, string Label)> RecognizeTokensWithLabels(string text)
    {
        var (encodingIds, encodingTokens) = _tokenizer.Encode(text);

        var tokenLabels = new List<(string Token, string Label)>();

        foreach (var (inputIds, tokens, _) in SplitIntoWindows(encodingIds, encodingTokens))
        {
            var predictions = PredictLabels(inputIds);
            for (int i = 0; i < tokens.Count; i++)
            {
                var token = Detokenize(tokens[i]);
                var label = _labels[predictions[i]];
                tokenLabels.Add((token, label));
            }
        }

        return tokenLabels;
    }

    public List<NamedEntity> RecognizeEntities(string text)
    {
        var (encodingIds, encodingTokens) = _tokenizer.Encode(text);

        var allTokens = new List<string>();
        var allLabels = new List<int>();

        foreach (var (inputIds, tokens, _) in SplitIntoWindows(encodingIds, encodingTokens))
        {
            var predictions = PredictLabels(inputIds);
            allTokens.AddRange(tokens);
            allLabels.AddRange(predictions);
        }

        var entities = new List<NamedEntity>();
        string? currentEntity = null;
        string? currentLabel = null;
        int entityStart = 0;

        for (int i = 0; i < allTokens.Count; i++)
        {
            var token = Detokenize(allTokens[i]);
            var label = _labels[allLabels[i]];

            if (label.StartsWith("B-"))
            {
                if (!string.IsNullOrEmpty(currentEntity))
                {
                    entities.Add(new NamedEntity { Text = currentEntity, Label = currentLabel, Start = entityStart, End = i });
                }
                currentEntity = token;
                currentLabel = label[2..];
                entityStart = i;
            }
            else if (label.StartsWith("I-") && currentLabel == label[2..])
            {
                currentEntity += " " + token;
            }
            else
            {
                if (!string.IsNullOrEmpty(currentEntity))
                {
                    entities.Add(new NamedEntity { Text = currentEntity, Label = currentLabel, Start = entityStart, End = i });
                    currentEntity = null;
                    currentLabel = null;
                }
            }
        }

        if (!string.IsNullOrEmpty(currentEntity))
        {
            entities.Add(new NamedEntity { Text = currentEntity, Label = currentLabel, Start = entityStart, End = allTokens.Count });
        }

        return entities;
    }

    private string Detokenize(string token) => token.Replace("Ġ", "").Replace("▁", "");
}

