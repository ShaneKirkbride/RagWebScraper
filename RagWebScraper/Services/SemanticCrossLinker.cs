using RagWebScraper.Models;

namespace RagWebScraper.Services
{
    public sealed class SemanticCrossLinker : ICrossDocumentLinker
    {
        private readonly IEmbeddingService _embedding;
        private const float SimilarityThreshold = 0.25f; // 0.92f;

        public SemanticCrossLinker(IEmbeddingService embedding)
        {
            _embedding = embedding;
        }

        public async Task<IEnumerable<LinkedPassage>> LinkAsync(IEnumerable<DocumentChunk> chunks)
        {
            var chunkList = chunks?.ToList() ?? new List<DocumentChunk>();
            if (chunkList.Count < 2)
                return Enumerable.Empty<LinkedPassage>();
            Console.WriteLine($"[Linker] Chunks to embed: {chunkList.Count}");

            var embeddings = await _embedding.GetEmbeddingsAsync(chunkList.Select(c => c.Text)).ConfigureAwait(false);
            Console.WriteLine($"[Linker] Embedding complete. Count: {embeddings.Count}");

            var results = new List<LinkedPassage>();

            for (int i = 0; i < chunkList.Count; i++)
            {
                for (int j = i + 1; j < chunkList.Count; j++)
                {
                    if (chunkList[i].SourceId == chunkList[j].SourceId)
                        continue;

                    float sim = CosineSimilarity(embeddings[i], embeddings[j]);
                    if (sim > SimilarityThreshold)
                    {
                        results.Add(new LinkedPassage(
                            chunkList[i].SourceId,
                            chunkList[i].Text,
                            chunkList[j].SourceId,
                            chunkList[j].Text,
                            sim));
                    }
                }
            }

            return results;
        }

        private static float CosineSimilarity(float[] a, float[] b)
        {
            float dot = 0, normA = 0, normB = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }

            return dot / (MathF.Sqrt(normA) * MathF.Sqrt(normB) + 1e-6f);
        }
    }


}
