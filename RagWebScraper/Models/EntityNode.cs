namespace RagWebScraper.Models;

public class EntityNode
{
    public EntityNode(string Id, string Label, string Type)
    {
        this.Id = Id;
        this.Label = Label;
        this.Type = Type;
    }

    public string Id { get; set; } // Unique name or GUID
    public string Label { get; set; }
    public string Type { get; set; } // PER, ORG, LOC, etc.
}
