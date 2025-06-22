using OpenAI.Chat;
using TourPlanner.config.Interfaces;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Model.Enums;

namespace TourPlanner.DAL.ServiceAgents;

public class AiService :IAiService
{
    private readonly ITourPlannerConfig _config;
    private readonly ILoggerWrapper _logger;
    
    public AiService(ITourPlannerConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = LoggerFactory.GetLogger<AiService>();
    }
    
    
    /// <summary>
    /// Answers a query using the AI model specified
    /// </summary>
    /// <param name="query">The query to be answered</param>
    /// <param name="model"><see cref="AiModel"/> to use for answering the query, defaults to GPT-4.1</param>
    /// <returns>The AI's response as a string</returns>
    public async Task<string> AnswerQueryAsync(string query, AiModel model = AiModel.GPT4_1)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            _logger.Warn("Query is null or empty. Returning empty response.");
            return String.Empty;
        }
        
        _logger.Debug($"Answering query '{query}' using model {model}...");
        
        try
        {
            ChatClient client = new ChatClient(model: GetModelStringFromEnum(model), apiKey: _config.OpenAiApiKey);
            ChatCompletion completion = await client.CompleteChatAsync(query);

            return completion.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to answer query '{query}' using model {model}.", ex);

            if (ex.Message.Contains("invalid_api_key"))
            {
                return "Invalid API key. Please check your OpenAI API key configuration.";
            }
            
            return String.Empty;
        }
    }
    
    
    /// <summary>
    /// Converts the <see cref="AiModel"/> enum to its corresponding string representation used by the OpenAI API
    /// </summary>
    /// <param name="model">The <see cref="AiModel"/> enum value to convert</param>
    /// <returns>The string representation of the AI model</returns>
    private string GetModelStringFromEnum(AiModel model)
    {
        return model switch
        {
            AiModel.GPT4_1 => "gpt-4.1-2025-04-14", // https://platform.openai.com/docs/models/gpt-4.1
            _ => throw new ArgumentOutOfRangeException(nameof(model), $"Unsupported AI model: {model}")
        };
    }
}