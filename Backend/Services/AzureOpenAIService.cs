using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using ColumbiaAi.Backend.Configuration;
using ColumbiaAi.Backend.Models;
using AppChatMessage = ColumbiaAi.Backend.Models.ChatMessage;

namespace ColumbiaAi.Backend.Services;

public interface IAzureOpenAIService
{
    Task<string> GetChatResponseAsync(List<AppChatMessage> messages, string? userContext = null);
}

public class AzureOpenAIService : IAzureOpenAIService
{
    private readonly AzureOpenAIClient _client;
    private readonly string _deploymentName;

    public AzureOpenAIService(AzureOpenAIConfig config)
    {
        _client = new AzureOpenAIClient(
            new Uri(config.Endpoint),
            new AzureKeyCredential(config.ApiKey)
        );
        _deploymentName = config.DeploymentName;
    }

    public async Task<string> GetChatResponseAsync(List<AppChatMessage> messages, string? userContext = null)
    {
        var chatClient = _client.GetChatClient(_deploymentName);
        var chatMessages = new List<OpenAI.Chat.ChatMessage>();

        if (!string.IsNullOrEmpty(userContext))
        {
            chatMessages.Add(new SystemChatMessage($"Context: {userContext}"));
        }

        chatMessages.Add(new SystemChatMessage("You are a helpful AI assistant. Provide thoughtful and accurate responses."));

        foreach (var msg in messages)
        {
            if (msg.Role == "user")
            {
                string content = msg.Content;

                if (msg.Attachments != null && msg.Attachments.Any())
                {
                    foreach (var url in msg.Attachments)
                    {
                        if (url.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            var pdfText = await PdfHelper.ExtractTextFromUrlAsync(url);
                            content += "\n\n[Attachment PDF Text]:\n" + pdfText;
                        }
                        else
                        {
                            // For plain text files
                            using var httpClient = new HttpClient();
                            var fileText = await httpClient.GetStringAsync(url);
                            content += "\n\n[Attachment Text]:\n" + fileText;
                        }
                    }
                }

                chatMessages.Add(new UserChatMessage(content));
            }
            else if (msg.Role == "assistant")
            {
                chatMessages.Add(new AssistantChatMessage(msg.Content));
            }
        }


        var response = await chatClient.CompleteChatAsync(chatMessages);
        return response.Value.Content[0].Text;
    }
}
