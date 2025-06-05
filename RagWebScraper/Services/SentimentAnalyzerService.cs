using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Tokenizers;

namespace RagWebScraper.Services
{
    public class SentimentAnalyzerService : ISentimentAnalyzer
    {
        private readonly MLContext _mlContext;
        private readonly PredictionEngine<SentimentInput, SentimentOutput> _predictionEngine;
        private readonly BertTokenizer _tokenizer;
        private readonly int _maxSequenceLength = 128;

        public SentimentAnalyzerService(IConfiguration config)
        {
            _mlContext = new MLContext();

            // Load tokenizer from vocab.txt (must match the ONNX model)
            var vocabPath = Path.Combine(AppContext.BaseDirectory, config["Sentiment:VocabPath"]);
            var modelPath = Path.Combine(AppContext.BaseDirectory, config["Sentiment:ModelPath"]);


            if (!File.Exists(vocabPath))
                throw new FileNotFoundException($"Tokenizer vocab file not found at {vocabPath}");

            _tokenizer = BertTokenizer.Create(vocabPath);

            if (!File.Exists(modelPath))
                throw new FileNotFoundException($"ONNX model not found at {modelPath}");

            var pipeline = _mlContext.Transforms.ApplyOnnxModel(
                outputColumnNames: new[] { "logits" },
                inputColumnNames: new[] { "input_ids", "attention_mask" },
                modelFile: modelPath,
                shapeDictionary: new Dictionary<string, int[]>()
                {
                    { "input_ids", new[] { 1, 128 } },
                    { "attention_mask", new[] { 1, 128 } }
                },
                gpuDeviceId: null,
                fallbackToCpu: true);
            // Provide dummy data with the correct input shape
            var dummyInput = new List<SentimentInput>
{
                new SentimentInput
                {
                    input_ids = Enumerable.Repeat(0L, 128).ToArray(),
                    attention_mask = Enumerable.Repeat(1L, 128).ToArray()
                }
            };

            var emptyData = _mlContext.Data.LoadFromEnumerable(dummyInput);
            var model = pipeline.Fit(emptyData);

            _predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentInput, SentimentOutput>(model);
        }

        public float AnalyzeSentiment(string text)
        {
            string? normalizedText;

            // Tokenize input text to tokens with IDs
            var encoding = _tokenizer.EncodeToTokens(text, out normalizedText);

            if (encoding == null || encoding.Count == 0)
                throw new InvalidOperationException($"Tokenization resulted in no tokens for input: '{text}'");

            var tokenIds = encoding.Select(t => t.Id).ToList();

            if (!tokenIds.Any())
                throw new InvalidOperationException($"Token IDs are empty for input: '{text}'");

            // Pad or truncate
            var inputIds = PadToFixedLength(tokenIds);
            var attentionMask = inputIds.Select(id => id == 0 ? 0L : 1L).ToArray();

            var input = new SentimentInput
            {
                input_ids = inputIds,
                attention_mask = attentionMask
            };

            var result = _predictionEngine.Predict(input);

            if (result?.logits == null || result.logits.Length < 2)
                throw new InvalidOperationException($"ONNX model returned invalid logits for input: '{text}'");

            float calibratedValue = 3.75F;
            var sentimentScore = result.logits[1] - result.logits[0] + calibratedValue;

            return sentimentScore;
        }

        private long[] PadToFixedLength(IEnumerable<int> input)
        {
            var list = input?.Take(_maxSequenceLength).ToList() ?? new List<int>();

            while (list.Count < _maxSequenceLength)
                list.Add(0);

            return list.Select(i => (long)i).ToArray();
        }

        public Dictionary<string, float> ExtractKeywordSentiments(string text, List<string> keywords)
        {
            var results = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(text) || keywords == null || keywords.Count == 0)
                return results;

            // Split text into sentences using simple delimiters (optional: use a real NLP tokenizer here)
            var sentences = text.Split(new[] { '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var keyword in keywords)
            {
                var matchingSentences = sentences
                    .Where(s => s.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                if (matchingSentences.Count == 0)
                {
                    results[keyword] = 0f; // neutral score if not found
                    continue;
                }

                var scoreSum = 0f;
                foreach (var sentence in matchingSentences)
                {
                    try
                    {
                        scoreSum += AnalyzeSentiment(sentence);
                    }
                    catch
                    {
                        // optionally log tokenization failure
                    }
                }

                var avg = scoreSum / matchingSentences.Count;
                results[keyword] = avg;
            }

            return results;
        }


        public class SentimentInput
        {
            [VectorType(1, 128)]
            public long[] input_ids { get; set; }

            [VectorType(1, 128)]
            public long[] attention_mask { get; set; }
        }

        public class SentimentOutput
        {
            [VectorType(1, 2)]
            public float[] logits { get; set; }
        }
    }
}
