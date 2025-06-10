namespace RagWebScraper.Services;

public interface ITokenizer
{
    (IReadOnlyList<int> Ids, IReadOnlyList<string> Tokens) Encode(string text);
}
