using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Chat;

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
    Task<(string, List<ChatMessage>)> ChatWithAgent(string userInput);
}

/// <summary>
/// Chat Agent to manage conversations with Azure OpenAI Service
/// </summary>
public class ChatAgent : IChatAgent
{
    private readonly ChatClient chatClient = null;
    ChatCompletionConfiguration chatCompletionConfiguration = null;
    ChatCompletionOptions requestOptions = null;

    /// <summary>
    /// Initialization
    /// </summary>
    /// <param name="config"></param>
    public ChatAgent(IConfiguration config)
    {
        Uri openaiEndpoint = new(config["AppSettings:AzureOpenAI:Endpoint"]);
        var openaiDeploymentName = config["AppSettings:AzureOpenAI:DeploymentName"];
        //var openaiApiKey = appSettings["AppSettings:AzureOpenAI:ApiKey"];
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
        var _client = new AzureOpenAIClient(openaiEndpoint, credential);
        // Initialize the AzureOpenAIClient with an API Key instead of user credentials...
        //_client = new(openaiEndpoint, new ApiKeyCredential(keyFromEnvironment));

        // Create a chat client
        chatClient = _client.GetChatClient(openaiDeploymentName);

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
                    Content = "You are going to be told a funny joke. " +
                    "It is your responsibility to describe that joke so that an artist can draw a picture of the mental image that this joke creates. " +
                    "Give clear instructions on how the scene should look and what objects should be included in the scene." +
                    "Instruct the artist to draw it in a humorous cartoon format."
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
    public async Task<(string, List<ChatMessage>)> ChatWithAgent(string userInput)
    {
        var chatConversation = new List<ChatMessage>
            {
               ChatMessage.CreateSystemMessage("You are a helpful assistant."),
               ChatMessage.CreateUserMessage(userInput)
            };

        // Get latest system message from AI configuration
        var chatMessages = new List<ChatMessage>(GetChatMessages(chatCompletionConfiguration));
        chatMessages.AddRange(chatConversation);

        // Get AI response and add it to chat conversation
        var response = await chatClient.CompleteChatAsync(chatMessages, requestOptions);

        string aiResponse = response.Value.Content[0].Text;
        chatConversation.Add(ChatMessage.CreateAssistantMessage(aiResponse));
        return (aiResponse, chatConversation);
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
