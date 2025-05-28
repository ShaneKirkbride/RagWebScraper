namespace RagWebScraper.Models
{
    public record AnalyzedDocument(string SourceId, List<DocumentChunk> Chunks);
}
