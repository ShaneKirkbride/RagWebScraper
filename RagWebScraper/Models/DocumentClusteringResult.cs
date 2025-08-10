using System;
using System.Collections.Generic;

namespace RagWebScraper.Models
{
    /// <summary>
    /// Represents the outcome of clustering documents, including assignments and metrics.
    /// </summary>
    public record DocumentClusteringResult(
        Dictionary<Guid, int> Clusters,
        ClusterMetrics Metrics);
}

