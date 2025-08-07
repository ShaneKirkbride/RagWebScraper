namespace RagWebScraper.Models;

/// <summary>
/// Configuration options governing file upload limits.
/// </summary>
public class FileUploadOptions
{
    /// <summary>
    /// Maximum size allowed for a single uploaded file in bytes.
    /// </summary>
    public long MaxFileSize { get; set; } = 1L * 1024 * 1024 * 1024; // 1 GB

    /// <summary>
    /// Maximum aggregate size allowed for a single request in bytes.
    /// </summary>
    public long MaxRequestSize { get; set; } = 10L * 1024 * 1024 * 1024; // 10 GB
}
