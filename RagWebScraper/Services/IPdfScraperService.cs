/// <summary>
/// Provides utilities for locating and downloading PDF files from the web.
/// </summary>
public interface IPdfScraperService
{
    /// <summary>
    /// Retrieves all PDF links from the specified page.
    /// </summary>
    /// <param name="url">URL of the page containing links.</param>
    Task<IEnumerable<string>> GetPdfLinksAsync(string url);

    /// <summary>
    /// Downloads the PDFs at the provided URLs.
    /// </summary>
    /// <param name="pdfUrls">URLs of the PDFs to download.</param>
    /// <param name="outputDirectory">Directory to place the downloaded files.</param>
    Task DownloadPdfsAsync(IEnumerable<string> pdfUrls, string outputDirectory);
}
