using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.AspNetCore.SignalR;
using OpenAI.Chat;
using OpenAI.Images;
using System.ClientModel;

// See https://learn.microsoft.com/en-us/azure/azure-app-configuration/quickstart-chat-completion-dotnet

namespace DadABase.Web.Helpers;

/// <summary>
/// Chat Agent Interface
/// </summary>
public interface IChatAgent
{
    /// <summary>
    /// Chat with AI Agent
    /// </summary>
    /// <param name="userInput"></param>
    /// <returns></returns>
    Task<(string, string, List<ChatMessage>)> ChatWithAgent(string userInput);
}

/// <summary>
/// Chat Agent to manage conversations with Azure OpenAI Service
/// </summary>
public class ChatAgent : IChatAgent
{
    private readonly ChatClient chatClient = null;
    private readonly ImageClient imageClient = null;
    private readonly ChatCompletionConfiguration chatCompletionConfiguration = null;
    private readonly ChatCompletionOptions requestOptions = null;

    /// <summary>
    /// Initialization
    /// </summary>
    /// <param name="config"></param>
    public ChatAgent(IConfiguration config)
    {
        Uri openaiEndpoint = new(config["AppSettings:AzureOpenAI:Endpoint"]);
        var openaiDeploymentName = config["AppSettings:AzureOpenAI:DeploymentName"];
        var openaiApiKey = config["AppSettings:AzureOpenAI:ApiKey"];

        Uri openaiImageEndpoint = new(config["AppSettings:AzureOpenAI:Image:Endpoint"]);
        var openaiImageDeploymentName = config["AppSettings:AzureOpenAI:Image:DeploymentName"];
        var openaiImageApiKey = config["AppSettings:AzureOpenAI:Image:ApiKey"];

        var openaiMaxTokens = int.TryParse(config["AppSettings:AzureOpenAI:MaxTokens"], out var parsedMaxTokens) ? parsedMaxTokens : 100;
        var openaiTemperature = float.TryParse(config["AppSettings:AzureOpenAI:Temperature"], out var parsedTemperature) ? parsedTemperature : 0.7f;
        float openaiTopP = float.TryParse(config["AppSettings:AzureOpenAI:TopP"], out var topP) ? topP :  0.95f;

        DefaultAzureCredential credential = string.IsNullOrEmpty(config["VisualStudioTenantId"]) ? new DefaultAzureCredential() :
            new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeEnvironmentCredential = true,
                ExcludeManagedIdentityCredential = true,
                TenantId = config["VisualStudioTenantId"] // if you get an error "Token tenant does not match resource tenant" during local development, force the tenant
            });

        // Initialize the AzureOpenAIClient
        var _chatClientHost = new AzureOpenAIClient(openaiEndpoint, credential);
        //var _chatClientHost = new(openaiEndpoint, new ApiKeyCredential(keyFromEnvironment));
        chatClient = _chatClientHost.GetChatClient(openaiDeploymentName);

        // Create an image generation client
        //var _imageClientHost = new AzureOpenAIClient(openaiImageEndpoint, credential);
        var _imageClientHost = new AzureOpenAIClient(openaiImageEndpoint, new ApiKeyCredential(openaiImageApiKey));
        imageClient = _imageClientHost.GetImageClient(openaiImageDeploymentName);

        // alternate way of doing this...
        //chatClient = new(
        //    model: openaiDeploymentName,
        //    credential: new ApiKeyCredential(openaiApiKey),
        //    options: new OpenAIClientOptions(){ Endpoint = openaiEndpoint }
        //);

        chatCompletionConfiguration = new ChatCompletionConfiguration
        {
            MaxTokens = openaiMaxTokens,
            Temperature = openaiTemperature,
            TopP = openaiTopP,
            Messages = [
                new() {
                    Role = "system",
                    Content = "You are going to be told a funny joke or a humorous line or an insightful quote. " +
                    "It is your responsibility to describe that joke so that an artist can draw a picture of the mental image that this joke creates. " +
                    "Give clear instructions on how the scene should look and what objects should be included in the scene." +
                    "Instruct the artist to draw it in a humorous cartoon format." +
                    "Make sure the description does not ask for anything violent, sexual, or political so that it does not violate safety rules."
                }
            ]
        };
        //Content = "You are the manager of a comedy club.  Evaluate this joke to see if it is funny and give your response, rating it on a humor scale from 1 to 10."

        requestOptions = new ChatCompletionOptions()
        {
            MaxOutputTokenCount = openaiMaxTokens,
            Temperature = openaiTemperature,
            TopP = openaiTopP
        };

    }

    // Helper method to convert configuration messages to ChatMessage objects
    private static IEnumerable<ChatMessage> GetChatMessages(ChatCompletionConfiguration chatCompletionConfiguration)
    {
        return chatCompletionConfiguration.Messages.Select<Message, ChatMessage>(message => message.Role switch
        {
            "system" => ChatMessage.CreateSystemMessage(message.Content),
            "user" => ChatMessage.CreateUserMessage(message.Content),
            "assistant" => ChatMessage.CreateAssistantMessage(message.Content),
            _ => throw new ArgumentException($"Unknown role: {message.Role}", nameof(message.Role))
        });
    }
    public async Task<(string, string, List<ChatMessage>)> ChatWithAgent(string userInput)
    {
        var chatConversation = new List<ChatMessage>
            {
               ChatMessage.CreateSystemMessage("You are a helpful assistant."),
               ChatMessage.CreateUserMessage(userInput)
            };

        // Get latest system message from AI configuration
        var chatMessages = new List<ChatMessage>(GetChatMessages(chatCompletionConfiguration));
        chatMessages.AddRange(chatConversation);
        string imageDescription = string.Empty;
        string imageUrl = string.Empty;
        bool startedImageGeneration = false;

        try
        {
            // Get AI response and add it to chat conversation
            var response = await chatClient.CompleteChatAsync(chatMessages, requestOptions);

            imageDescription = response.Value.Content[0].Text;
            chatConversation.Add(ChatMessage.CreateAssistantMessage(imageDescription));
            Console.WriteLine($"Image description {imageDescription}");

            startedImageGeneration = true;
            var imageResult = await imageClient.GenerateImageAsync(imageDescription, new()
            {
                Quality = GeneratedImageQuality.Standard,
                Size = GeneratedImageSize.W1024xH1024,
                Style = GeneratedImageStyle.Vivid,
                ResponseFormat = GeneratedImageFormat.Uri
            });

            var image = imageResult.Value;
            imageUrl = image.ImageUri.ToString();
            Console.WriteLine($"Image URL {imageUrl}");

            chatConversation.Add(ChatMessage.CreateAssistantMessage(imageUrl));

            return (imageDescription, imageUrl, chatConversation);
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            Console.WriteLine($"Error during chat completion or image generation {errorMessage}");
            return (imageDescription, startedImageGeneration ? errorMessage : imageUrl, chatConversation);
        }
    }
    internal class ChatCompletionConfiguration
    {
        [ConfigurationKeyName("model")]
        public string Model { get; set; }

        [ConfigurationKeyName("messages")]
        public List<Message> Messages { get; set; }

        [ConfigurationKeyName("max_tokens")]
        public int MaxTokens { get; set; }

        [ConfigurationKeyName("temperature")]
        public float Temperature { get; set; }

        [ConfigurationKeyName("top_p")]
        public float TopP { get; set; }
    }

    internal class Message
    {
        [ConfigurationKeyName("role")]
        public required string Role { get; set; }

        [ConfigurationKeyName("content")]
        public string Content { get; set; }
    }
}
