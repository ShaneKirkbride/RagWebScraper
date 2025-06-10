namespace RagWebScraper.Models;
public class EntityGraphResult
{
    public List<EntityNode> Nodes { get; set; } = new();
    public List<EntityEdge> Edges { get; set; } = new();
    /// <summary>
    /// Sentences annotated with XML tags for each token label.
    /// </summary>
    public List<string> LabeledSentences { get; set; } = new();
}