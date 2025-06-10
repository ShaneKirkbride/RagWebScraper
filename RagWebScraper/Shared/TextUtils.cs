namespace RagWebScraper.Shared;

public static class TextUtils
{
    public static string Truncate(string value, int maxLength = 50)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Length <= maxLength
            ? value
            : value[..maxLength] + "...";
    }
}
