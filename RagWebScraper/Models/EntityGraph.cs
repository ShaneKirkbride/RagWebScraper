namespace RagWebScraper.Models
{
    public record EntityGraph(string SourceId, List<EntityNode> Nodes, List<EntityEdge> Edges);

}
