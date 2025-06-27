using OpenAI;
using OpenAI.Chat;

namespace RagWebScraper.Services;

/// <summary>
/// OpenAI-based implementation of <see cref="IChatCompletionService"/>.
/// </summary>
public sealed class OpenAIChatCompletionService : IChatCompletionService
{
    private readonly OpenAIClient _client;

    public OpenAIChatCompletionService(OpenAIClient client)
    {
        _client = client;
    }

    public async Task<string> GetCompletionAsync(string systemPrompt, string userPrompt)
    {
        var chat = _client.GetChatClient("gpt-4");
        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(systemPrompt),
            ChatMessage.CreateUserMessage(userPrompt)
        };
        ChatCompletion response = await chat.CompleteChatAsync(messages);
        return response.Content[0].Text ?? string.Empty;
    }
}
