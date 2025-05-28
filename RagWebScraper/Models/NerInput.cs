using Microsoft.ML.Data;

namespace RagWebScraper.Models
{
    public class NerInput
    {
        [VectorType(1, 128)]
        public long[] input_ids { get; set; }

        [VectorType(1, 128)]
        public long[] attention_mask { get; set; }

        [VectorType(1, 128)]
        public long[] token_type_ids { get; set; }  // ✅ Required!
    }
}
