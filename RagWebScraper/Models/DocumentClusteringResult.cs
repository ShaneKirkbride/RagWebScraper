using System;
using System.Collections.Generic;

namespace RagWebScraper.Models
{
    /// <summary>
    /// Represents the outcome of clustering documents, including assignments,
    /// metrics, and descriptive information for each cluster.
    /// </summary>
    public record DocumentClusteringResult(
        Dictionary<Guid, int> Clusters,
        ClusterMetrics Metrics,
        IReadOnlyList<ClusterDescriptor> Descriptors);
}

