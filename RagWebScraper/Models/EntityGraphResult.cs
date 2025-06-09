namespace RagWebScraper.Models;
public class EntityGraphResult
{
    public List<EntityNode> Nodes { get; set; } = new();
    public List<EntityEdge> Edges { get; set; } = new();
}