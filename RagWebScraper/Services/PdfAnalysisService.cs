namespace RagWebScraper.Services;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

public class PdfAnalysisService : IAnalysisService
{
    public async Task AnalyzePdfAsync(string pdfFilePath)
    {
        if (!File.Exists(pdfFilePath))
        {
            throw new FileNotFoundException("PDF file not found.", pdfFilePath);
        }

        var sb = new StringBuilder();

        using (var document = PdfDocument.Open(pdfFilePath))
        {
            foreach (var page in document.GetPages())
            {
                sb.AppendLine(page.Text);
            }
        }

        var outputText = sb.ToString();

        // For demonstration, write the extracted text to a .txt file
        var outputPath = Path.ChangeExtension(pdfFilePath, ".txt");
        await File.WriteAllTextAsync(outputPath, outputText);

        // Further analysis (e.g., sentiment analysis, keyword extraction) can be performed here
    }
}