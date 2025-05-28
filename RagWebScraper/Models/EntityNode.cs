namespace RagWebScraper.Models;

public record EntityNode(
    string Id,              // Unique ID or name of the entity
    string Label,           // The display label (e.g. "Laser Diode")
    string Type             // Optional: "Person", "Concept", "Device", etc.
);
