using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Chat;
using OpenAI.Images;
using System.ClientModel;

// See https://learn.microsoft.com/en-us/azure/azure-app-configuration/quickstart-chat-completion-dotnet

namespace DadABase.Web.Repositories;

/// <summary>
/// Chat Agent to manage conversations with Azure OpenAI Service
/// </summary>
public class AIHelper : IAIHelper
{
    private readonly Uri openaiEndpoint = null;
    private readonly string openaiDeploymentName = "gpt-4o";
    private readonly string openaiApiKey = string.Empty;

    private readonly Uri openaiImageEndpoint = null;
    private readonly string openaiImageDeploymentName = "dall-e-3";
    private readonly string openaiImageApiKey = string.Empty;

    private readonly int openaiMaxTokens = 100;
    private readonly float openaiTemperature = 0.7f;
    private readonly float openaiTopP = 0.95f;

    private ChatClient chatClient = null;
    private ChatCompletionConfiguration chatCompletionConfiguration = null;
    private ChatCompletionOptions chatRequestOptions = null;

    private ImageClient imageGenerator = null;

    private const string JokeImageGeneratorPrompt =
        "You are going to be told a funny joke or a humorous line or an insightful quote. " +
        "It is your responsibility to describe that joke so that an artist can draw a picture of the mental image that this joke creates. " +
        "Give clear instructions on how the scene should look and what objects should be included in the scene." +
        "Instruct the artist to draw it in a humorous cartoon format." +
        "Make sure the description does not ask for anything violent, sexual, or political so that it does not violate safety rules.";

    DefaultAzureCredential credential = null;

    /// <summary>
    /// Initialization
    /// </summary>
    /// <param name="config"></param>
    public AIHelper(IConfiguration config)
    {
        openaiEndpoint = new(config["AppSettings:AzureOpenAI:Chat:Endpoint"]);
        openaiDeploymentName = config["AppSettings:AzureOpenAI:Chat:DeploymentName"];
        openaiApiKey = config["AppSettings:AzureOpenAI:Chat:ApiKey"];
        openaiMaxTokens = int.TryParse(config["AppSettings:AzureOpenAI:Chat:MaxTokens"], out var parsedMaxTokens) ? parsedMaxTokens : 100;
        openaiTemperature = float.TryParse(config["AppSettings:AzureOpenAI:Chat:Temperature"], out var parsedTemperature) ? parsedTemperature : 0.7f;
        openaiTopP = float.TryParse(config["AppSettings:AzureOpenAI:Chat:TopP"], out var topP) ? topP : 0.95f;

        openaiImageEndpoint = new(config["AppSettings:AzureOpenAI:Image:Endpoint"]);
        openaiImageDeploymentName = config["AppSettings:AzureOpenAI:Image:DeploymentName"];
        openaiImageApiKey = config["AppSettings:AzureOpenAI:Image:ApiKey"];

        credential = string.IsNullOrEmpty(config["VisualStudioTenantId"]) ? new DefaultAzureCredential() :
            new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeEnvironmentCredential = true,
                ExcludeManagedIdentityCredential = true,
                TenantId = config["VisualStudioTenantId"] // if you get an error "Token tenant does not match resource tenant" during local development, force the tenant
            });
    }

    /// <summary>
    /// Initialize the chat agent
    /// </summary>
    private void InitializeChatAgent(DefaultAzureCredential credential)
    {
        if (chatClient != null) return;

        var _chatClientHost = new AzureOpenAIClient(openaiEndpoint, credential);
        //  -OR-  var _chatClientHost = new(openaiEndpoint, new ApiKeyCredential(keyFromEnvironment));
        chatClient = _chatClientHost.GetChatClient(openaiDeploymentName);

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
                    Content = JokeImageGeneratorPrompt
                }
            ]
        };
        chatRequestOptions = new ChatCompletionOptions()
        {
            MaxOutputTokenCount = openaiMaxTokens,
            Temperature = openaiTemperature,
            TopP = openaiTopP
        };
    }

    /// <summary>
    /// Initialize the Image Generator
    /// </summary>
    private void InitializeImageGenerator(DefaultAzureCredential credential)
    {
        if (imageGenerator != null) return;

        // Create an image generation client
        var _imageClientHost = new AzureOpenAIClient(openaiImageEndpoint, new ApiKeyCredential(openaiImageApiKey));
        //  -OR-  var _imageClientHost = new AzureOpenAIClient(openaiImageEndpoint, credential);
        imageGenerator = _imageClientHost.GetImageClient(openaiImageDeploymentName);
    }

    /// <summary>
    /// Give it a joke and get back an image description
    /// </summary>
    /// <returns></returns>
    public async Task<(string description, bool success, string message)> GetJokeImageDescription(string jokeText)
    {
        string imageDescription = string.Empty;

        try
        {
            InitializeChatAgent(credential);

            var chatConversation = new List<ChatMessage>
            {
               ChatMessage.CreateSystemMessage(JokeImageGeneratorPrompt),
               ChatMessage.CreateUserMessage(jokeText)
            };
            // Get latest system message from AI configuration
            var chatMessages = new List<ChatMessage>(GetChatMessages(chatCompletionConfiguration));
            chatMessages.AddRange(chatConversation);

            // Get AI response and add it to chat conversation
            var response = await chatClient.CompleteChatAsync(chatMessages, chatRequestOptions);

            imageDescription = response.Value.Content[0].Text;
            chatConversation.Add(ChatMessage.CreateAssistantMessage(imageDescription));
            Console.WriteLine($"Joke: {jokeText} \nImage description {imageDescription}");
            return (imageDescription, true, string.Empty);
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            Console.WriteLine($"Error during description generation: {errorMessage}");
            return (imageDescription, false, errorMessage);
        }
    }

    /// <summary>
    /// Give this a description and get back an generated image URL
    /// </summary>
    /// <returns></returns>
    public async Task<(string, bool, string)> GenerateAnImage(string imageDescription)
    {
        var imageUrl = string.Empty;
        try
        {
            InitializeImageGenerator(credential);

            var imageResult = await imageGenerator.GenerateImageAsync(imageDescription, new()
            {
                Quality = GeneratedImageQuality.Standard,
                Size = GeneratedImageSize.W1024xH1024,
                Style = GeneratedImageStyle.Vivid,
                ResponseFormat = GeneratedImageFormat.Uri
            });
            var image = imageResult.Value;
            imageUrl = image.ImageUri.ToString();
            Console.WriteLine($"Generated Image URL {imageUrl}");
            return (imageUrl, true, string.Empty);
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            Console.WriteLine($"Error during image generation: {errorMessage}");
            var sorryMessage = "Sorry - I can't even imagine drawing that picture...!  Try again with a different joke!";
            if (errorMessage.Contains("safety system", StringComparison.CurrentCultureIgnoreCase))
            {
                errorMessage = $"{sorryMessage} (safety violation)";
            }
            if (errorMessage.Contains("content filter", StringComparison.CurrentCultureIgnoreCase))
            {
                errorMessage = $"{sorryMessage} (content filter violation)";
            }
            return (imageDescription, false, errorMessage);
        }
    }

    /// <summary>
    /// Helper method to convert configuration messages to ChatMessage objects
    /// </summary>
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
