namespace RagWebScraper.Services
{
    public class IngestionService
    {
        private readonly IPdfScraperService _pdfScraperService;
        private readonly IAnalysisService _analysisService;

        public IngestionService(IPdfScraperService pdfScraperService, IAnalysisService analysisService)
        {
            _pdfScraperService = pdfScraperService;
            _analysisService = analysisService;
        }

        public async Task IngestPdfsFromWebsiteAsync(string websiteUrl, string outputDirectory)
        {
            var pdfLinks = await _pdfScraperService.GetPdfLinksAsync(websiteUrl);
            await _pdfScraperService.DownloadPdfsAsync(pdfLinks, outputDirectory);

            foreach (var pdfPath in Directory.GetFiles(outputDirectory, "*.pdf"))
            {
                await _analysisService.AnalyzePdfAsync(pdfPath);
            }
        }
    }
}
