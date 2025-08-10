using System;

namespace RagWebScraper.Models
{
    /// <summary>
    /// Describes quality metrics for a clustering operation.
    /// </summary>
    public record ClusterMetrics(
        double AverageDistance,
        double DaviesBouldinIndex,
        double NormalizedMutualInformation);
}

