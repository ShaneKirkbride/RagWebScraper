using System.Text.RegularExpressions;

namespace RagWebScraper.Services;

public static class SentenceSplitter
{
    public static List<string> Split(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        string abbrev = @"(?:Mr|Mrs|Ms|Dr|Prof|Sr|Jr|vs|etc|e\.g|i\.e|U\.S|U\.K|Inc|Ltd|St|Mt|No)\.";
        string pattern = $@"(?<!\b{abbrev})(?<=[.!?])(?=\s?[A-Z])";

        return Regex.Split(text, pattern, RegexOptions.IgnoreCase)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .ToList();
    }
}
