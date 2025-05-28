using RagWebScraper.Models;

namespace RagWebScraper.Services
{
    public interface ICrossDocumentLinker
    {
        Task<IEnumerable<LinkedPassage>> LinkAsync(IEnumerable<DocumentChunk> chunks);
    }
}
