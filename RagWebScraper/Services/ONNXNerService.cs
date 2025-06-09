using Microsoft.ML.Tokenizers;
using Microsoft.ML;
using RagWebScraper.Models;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;

namespace RagWebScraper.Services;

public class ONNXNerService : INerService
{
    private readonly EnglishRobertaTokenizer _tokenizer;
    private readonly InferenceSession _session;
    private readonly string[] _labels =
    {
        "O", "B-PER", "I-PER", "B-ORG", "I-ORG", "B-LOC", "I-LOC", "B-MISC", "I-MISC"
    };

    public ONNXNerService(string modelPath, string vocabPath, string mergesPath, string dictionaryPath)
    {
        _tokenizer = EnglishRobertaTokenizer.Create(vocabPath, mergesPath, dictionaryPath);
        _session = new InferenceSession(modelPath);
    }
    
    public List<(string Token, string Label)> RecognizeTokensWithLabels(string text)
    {
        var encodingIds = _tokenizer.EncodeToIds(text);
        var encodingTokens = _tokenizer.EncodeToTokens(text, out _);

        long[] inputIds = encodingIds.Select(id => (long)id).ToArray();
        long[] attentionMask = Enumerable.Repeat(1L, inputIds.Length).ToArray();

        var inputIdsTensor = new DenseTensor<long>(inputIds, new[] { 1, inputIds.Length });
        var attentionMaskTensor = new DenseTensor<long>(attentionMask, new[] { 1, attentionMask.Length });

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor)
        };

        using var results = _session.Run(inputs);
        var logits = results.First().AsEnumerable<float>().ToArray();

        int tokenCount = inputIds.Length;
        int labelCount = logits.Length / tokenCount;

        var tokenLabels = new List<(string Token, string Label)>();

        for (int i = 0; i < tokenCount; i++)
        {
            int offset = i * labelCount;
            var tokenLogits = logits.Skip(offset).Take(labelCount).ToArray();
            int predictedIdx = Array.IndexOf(tokenLogits, tokenLogits.Max());

            var token = Detokenize(encodingTokens[i].Value);
            var label = _labels[predictedIdx];

            tokenLabels.Add((token, label));
        }

        return tokenLabels;
    }

    public List<NamedEntity> RecognizeEntities(string text)
    {
        var encodingIds = _tokenizer.EncodeToIds(text);
        var encodingTokens = _tokenizer.EncodeToTokens(text, out _);

        long[] inputIds = encodingIds.Select(id => (long)id).ToArray();
        long[] attentionMask = Enumerable.Repeat(1L, inputIds.Length).ToArray();

        var inputIdsTensor = new DenseTensor<long>(inputIds, new[] { 1, inputIds.Length });
        var attentionMaskTensor = new DenseTensor<long>(attentionMask, new[] { 1, attentionMask.Length });

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor)
        };

        using var results = _session.Run(inputs);
        var logits = results.First().AsEnumerable<float>().ToArray();

        int tokenCount = inputIds.Length;
        int labelCount = logits.Length / tokenCount;
        var labelIds = new List<int>();

        for (int i = 0; i < tokenCount; i++)
        {
            int offset = i * labelCount;
            var tokenLogits = logits.Skip(offset).Take(labelCount).ToArray();
            int predictedIdx = Array.IndexOf(tokenLogits, tokenLogits.Max());
            labelIds.Add(predictedIdx);
        }

        var entities = new List<NamedEntity>();
        string? currentEntity = null;
        string? currentLabel = null;
        int entityStart = 0;

        for (int i = 0; i < encodingTokens.Count; i++)
        {
            var token = Detokenize(encodingTokens[i].Value);
            var label = _labels[labelIds[i]];

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
            entities.Add(new NamedEntity { Text = currentEntity, Label = currentLabel, Start = entityStart, End = encodingTokens.Count });
        }

        return entities;
    }

    private string Detokenize(string token) => token.Replace("Ġ", "").Replace("▁", "");
}

