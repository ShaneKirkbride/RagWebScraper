namespace RagWebScraper.Models
{
    public class RagAnalysisResult
    {
        public RagAnalysisResult(IEnumerable<LinkedPassage> links, List<EntityGraph> entityGraphs)
        {
            Links = links;
            EntityGraphs = entityGraphs;
        }

        public IEnumerable<LinkedPassage> Links { get; }
        public List<EntityGraph> EntityGraphs { get; }
    }

}
