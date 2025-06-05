using System.Text;
using UglyToad.PdfPig;

namespace RagWebScraper.Services
{
    public class PdfTextExtractorService : ITextExtractor
    {
        public string ExtractText(Stream pdfStream)
        {
            using var document = PdfDocument.Open(pdfStream);
            var builder = new StringBuilder();
            foreach (var page in document.GetPages())
            {
                builder.AppendLine(page.Text);
            }
            return builder.ToString();
        }
    }
}
