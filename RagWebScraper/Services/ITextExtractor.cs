namespace RagWebScraper.Services;

/// <summary>
/// Extracts plain text from PDF streams.
/// </summary>
public interface ITextExtractor
{
    /// <summary>
    /// Reads text from the provided PDF stream.
    /// </summary>
    /// <param name="pdfStream">The PDF file stream.</param>
    /// <returns>The extracted plain text.</returns>
    Task<string> ExtractTextAsync(Stream pdfStream);
}
