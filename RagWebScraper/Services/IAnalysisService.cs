namespace RagWebScraper.Services;

/// <summary>
/// Exposes operations to analyze PDF files.
/// </summary>
public interface IAnalysisService
{
    /// <summary>
    /// Analyzes the specified PDF file.
    /// </summary>
    /// <param name="pdfFilePath">The file path of the PDF to analyze.</param>
    Task AnalyzePdfAsync(string pdfFilePath);
}
