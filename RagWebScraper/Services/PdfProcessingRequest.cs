namespace RagWebScraper.Services
{
    /// <summary>
    /// Represents a queued PDF to process. The file is persisted to a temporary
    /// path on disk to avoid holding large streams in memory.
    /// </summary>
    public class PdfProcessingRequest
    {
        /// <summary>Friendly file name originally uploaded.</summary>
        public string FileName { get; init; } = string.Empty;

        /// <summary>Temporary file path of the uploaded PDF.</summary>
        public string FilePath { get; init; } = string.Empty;

        /// <summary>List of search keywords for analysis.</summary>
        public List<string> Keywords { get; init; } = new();
    }

}