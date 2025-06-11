using RagWebScraper.Models;

namespace RagWebScraper.Services
{
    /// <summary>
    /// Provides functionality to cluster documents by topic.
    /// </summary>
    public interface IDocumentClusterer
    {
        /// <summary>
        /// Clusters the supplied documents into the specified number of groups.
        /// </summary>
        /// <param name="documents">Documents to cluster.</param>
        /// <param name="numberOfClusters">Desired number of clusters.</param>
        /// <returns>Mapping of document ID to cluster ID.</returns>
        Task<Dictionary<Guid, int>> ClusterAsync(IEnumerable<Document> documents, int numberOfClusters = 5);
    }
}
