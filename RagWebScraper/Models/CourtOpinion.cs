namespace RagWebScraper.Models;

/// <summary>
/// Represents a single court opinion from the CourtListener API.
/// </summary>
public sealed class CourtOpinion
{
    /// <summary>
    /// Opinion identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Case name for the opinion.
    /// </summary>
    public string CaseName { get; set; } = string.Empty;

    /// <summary>
    /// Plain text of the opinion.
    /// </summary>
    public string PlainText { get; set; } = string.Empty;
}
