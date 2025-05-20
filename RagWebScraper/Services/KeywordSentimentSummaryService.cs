using OpenAI;
using OpenAI.Chat;
using RagWebScraper.Models;
using System.Text;
using System.Text.Json;

namespace RagWebScraper.Services
{
    public class KeywordSentimentSummaryService
    {
        private readonly OpenAIClient _openai;

        public KeywordSentimentSummaryService(OpenAIClient openai)
        {
            _openai = openai;
        }

        public async Task<string> GenerateSummaryAsync(List<AnalysisResult> results)
        {
            if (results == null || results.Count == 0)
                return "No data provided.";

            var aggregated = new Dictionary<string, List<float>>();

            foreach (var result in results)
            {
                if (result.KeywordSentimentScores == null) continue;

                foreach (var kv in result.KeywordSentimentScores)
                {
                    if (!aggregated.ContainsKey(kv.Key))
                        aggregated[kv.Key] = new();

                    aggregated[kv.Key].Add(kv.Value);
                }
            }

            var averaged = aggregated.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Average());

            var prompt = BuildPromptFromAveragedSentiments(averaged);

            var chatClient = _openai.GetChatClient("gpt-4");
            var chatMessage = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage("You are an analyst. Provide a short summary of the keyword-level sentiment results."),
                ChatMessage.CreateUserMessage(prompt)
            };

            ChatCompletion response = await chatClient.CompleteChatAsync(chatMessage);

            return response.Content[0].Text ?? "No summary was generated.";
        }

        private string BuildPromptFromAveragedSentiments(Dictionary<string, float> averaged)
        {
            var summaryLines = averaged
                .OrderByDescending(kv => Math.Abs(kv.Value))
                .Select(kv => $"{kv.Key}: {kv.Value:+0.00;-0.00}");

            return $"Here is the keyword sentiment data:\n{string.Join("\n", summaryLines)}\n\nPlease summarize it.";
        }
    }

}
