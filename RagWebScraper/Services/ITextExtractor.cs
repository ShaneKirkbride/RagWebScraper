namespace RagWebScraper.Services
{
    public interface ITextExtractor
    {
        string ExtractText(Stream pdfStream);
    }

}