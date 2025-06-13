namespace RagWebScraper.Services
{
    using System;
    using System.Collections.Generic;
    using RagWebScraper.Models;

    /// <summary>
    /// Provides equality comparison for <see cref="AnalysisResult"/> objects.
    /// </summary>
    public class AnalysisResultComparer : IEqualityComparer<AnalysisResult>
    {
        /// <summary>
        /// Determines if two analysis results are equal.
        /// </summary>
        public bool Equals(AnalysisResult? x, AnalysisResult? y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x is null || y is null)
                return false;

            return string.Equals(x.Url, y.Url, StringComparison.OrdinalIgnoreCase) &&
                   x.PageSentimentScore.Equals(y.PageSentimentScore) &&
                   DictionaryEquals(x.KeywordFrequencies, y.KeywordFrequencies) &&
                   DictionaryEquals(x.KeywordSentimentScores, y.KeywordSentimentScores) &&
                   string.Equals(x.KeywordSummary, y.KeywordSummary, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Computes a hash code for the given analysis result.
        /// </summary>
        /// <param name="obj">The result to compute a hash for.</param>
        /// <returns>The calculated hash code.</returns>
        public int GetHashCode(AnalysisResult obj)
        {
            if (obj is null)
                return 0;

            int hash = obj.Url?.ToLowerInvariant().GetHashCode() ?? 0;
            hash = (hash * 397) ^ obj.PageSentimentScore.GetHashCode();
            hash = (hash * 397) ^ GetDictionaryHashCode(obj.KeywordFrequencies);
            hash = (hash * 397) ^ GetDictionaryHashCode(obj.KeywordSentimentScores);
            hash = (hash * 397) ^ (obj.KeywordSummary?.ToLowerInvariant().GetHashCode() ?? 0);
            return hash;
        }

        /// <summary>
        /// Compares two dictionaries for equality.
        /// </summary>
        private bool DictionaryEquals<TKey, TValue>(
            IDictionary<TKey, TValue>? dict1,
            IDictionary<TKey, TValue>? dict2)
        {
            if (ReferenceEquals(dict1, dict2))
                return true;

            if (dict1 is null || dict2 is null || dict1.Count != dict2.Count)
                return false;

            foreach (var kvp in dict1)
            {
                if (!dict2.TryGetValue(kvp.Key, out var value) ||
                    !EqualityComparer<TValue>.Default.Equals(kvp.Value, value))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Generates a hash code for a dictionary instance.
        /// </summary>
        private int GetDictionaryHashCode<TKey, TValue>(IDictionary<TKey, TValue>? dictionary)
        {
            if (dictionary is null)
                return 0;

            int hash = 0;
            foreach (var kvp in dictionary)
            {
                int keyHash = kvp.Key?.GetHashCode() ?? 0;
                int valueHash = kvp.Value?.GetHashCode() ?? 0;
                hash ^= keyHash ^ valueHash;
            }
            return hash;
        }
    }

}
