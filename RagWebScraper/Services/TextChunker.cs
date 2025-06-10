namespace RagWebScraper.Services
{
    public class TextChunker
    {
        /// <summary>
        /// Splits the text into chunks of approximately the given max token length.
        /// Basic naive approach: splits by sentence, then batches.
        /// </summary>
        /// <param name="text">The full text to chunk.</param>
        /// <param name="maxTokensPerChunk">Approx max tokens (words) per chunk.</param>
        /// <returns>List of text chunks.</returns>
        public List<string> ChunkText(string text, int maxTokensPerChunk = 500)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            var sentences = SentenceSplitter.Split(text);

            var chunks = new List<string>();
            var currentChunk = new List<string>();
            var currentTokenCount = 0;

            foreach (var sentence in sentences)
            {
                var sentenceTokenCount = CountTokens(sentence);

                if (currentTokenCount + sentenceTokenCount > maxTokensPerChunk && currentChunk.Any())
                {
                    chunks.Add(string.Join(" ", currentChunk));
                    currentChunk.Clear();
                    currentTokenCount = 0;
                }

                currentChunk.Add(sentence);
                currentTokenCount += sentenceTokenCount;
            }

            if (currentChunk.Any())
                chunks.Add(string.Join(" ", currentChunk));

            return chunks;
        }

        /// <summary>
        /// Naive token counter (splits by whitespace). You can refine this using more precise tokenizers (e.g., tiktoken).
        /// </summary>
        private int CountTokens(string text)
        {
            return text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
        }

}
}
