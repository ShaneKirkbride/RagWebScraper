using RagWebScraper.Models;

namespace RagWebScraper.Services
{
    public interface IEntityGraphExtractor
    {
        EntityGraph Extract(string text, string sourceId);
    }
}
