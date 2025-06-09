namespace RagWebScraper.Models;

public class EntityEdge
{
    public EntityEdge(string SourceId, string TargetId, string Relation)
    {
        this.SourceId = SourceId;
        this.TargetId = TargetId;
        this.Relation = Relation;
    }

    public string SourceId { get; set; }
    public string TargetId { get; set; }
    public string Relation { get; set; }
}