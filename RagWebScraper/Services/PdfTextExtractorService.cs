namespace RagWebScraper.Services;

using System.Text;
using UglyToad.PdfPig;
public class PdfTextExtractorService : ITextExtractor
{
    public string ExtractText(Stream pdfStream)
    {
        using var document = PdfDocument.Open(pdfStream);
        var builder = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            var words = page.GetWords();
            if (words.Any())
            {
                foreach (var word in words)
                {
                    builder.Append(word.Text);
                    builder.Append(' ');
                }
            }
            else
            {
                // fallback to letters if words are missing
                var letters = page.Letters;
                for (int i = 0; i < letters.Count; i++)
                {
                    builder.Append(letters[i].Value);

                    if (i < letters.Count - 1)
                    {
                        var current = letters[i];
                        var next = letters[i + 1];
                        var distance = next.StartBaseLine.X - current.EndBaseLine.X;

                        if (distance > current.FontSize * 0.25)
                        {
                            builder.Append(' ');
                        }
                    }
                }
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }
}
