public interface IPdfScraperService
{
    Task<IEnumerable<string>> GetPdfLinksAsync(string url);
    Task DownloadPdfsAsync(IEnumerable<string> pdfUrls, string outputDirectory);
}
