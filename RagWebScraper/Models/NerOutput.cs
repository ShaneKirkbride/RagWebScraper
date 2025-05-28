using Microsoft.ML.Data;

namespace RagWebScraper.Models
{
    public class NerOutput
    {
        [ColumnName("logits")]
        //[VectorType(1, 128)] // e.g., N = number of labels like 9
        public float[] logits { get; set; }
    }
}
