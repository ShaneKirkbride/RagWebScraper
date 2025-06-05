using Microsoft.AspNetCore.Mvc;
using RagWebScraper.Services;

namespace RagWebScraper.Pages
{
    public class FileSentimentSummaryComparer : IEqualityComparer<FileSentimentSummary>
    {
        public bool Equals(FileSentimentSummary? x, FileSentimentSummary? y)
        {
            if (x == null || y == null) return false;
            return x.FileName == y.FileName &&
                   x.Sentiment == y.Sentiment &&
                   x.RawText == y.RawText;
        }

        public int GetHashCode(FileSentimentSummary obj) =>
            HashCode.Combine(obj.FileName, obj.Sentiment, obj.RawText);
    }
}
