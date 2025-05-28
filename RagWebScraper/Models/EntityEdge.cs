namespace RagWebScraper.Models;

public record EntityEdge(
    string SourceId,        // Node ID
    string TargetId,        // Node ID
    string Relation         // e.g. "uses", "is part of", "connected to"
);
