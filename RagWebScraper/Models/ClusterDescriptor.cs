using System.Collections.Generic;

namespace RagWebScraper.Models
{
    /// <summary>
    /// Provides descriptive information about a single cluster.
    /// </summary>
    public record ClusterDescriptor(
        int ClusterId,
        IReadOnlyList<string> TopWords,
        string Reason);
}
