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
        /// <returns>Cluster assignments and associated metrics.</returns>
        Task<DocumentClusteringResult> ClusterAsync(IEnumerable<Document> documents, int numberOfClusters = 5);
    }
}
