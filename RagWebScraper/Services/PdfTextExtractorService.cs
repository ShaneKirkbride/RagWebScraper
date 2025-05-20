using System.Text;
using UglyToad.PdfPig;

namespace RagWebScraper.Services
{
    public class PdfTextExtractorService
    {
        public string ExtractText(Stream pdfStream)
        {
            using var document = PdfDocument.Open(pdfStream);
            var textBuilder = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                textBuilder.AppendLine(page.Text);
            }

            return textBuilder.ToString();
        }
    }
}
