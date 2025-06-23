using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TourPlanner.config.Interfaces;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Model.Enums;

namespace TourPlanner.DAL.ServiceAgents;

public class AiService :IAiService
{
    private readonly ITourPlannerConfig _config;
    private readonly HttpClient _httpClient;
    private readonly ILoggerWrapper _logger;
    
    public AiService(ITourPlannerConfig config, HttpClient httpClient)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = LoggerFactory.GetLogger<AiService>();
        
        // Set the API Key in the HTTP client headers
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.OpenRouterApiKey}");
    }
    
    
    /// <summary>
    /// Answers a query using the AI model specified
    /// </summary>
    /// <param name="systemPrompt">The system prompt to provide context for the AI</param>
    /// <param name="query">The query to be answered</param>
    /// <param name="model"><see cref="AiModel"/> to use for answering the query, defaults to GPT-4.1</param>
    /// <returns>The AI's response as a string</returns>
    public async Task<string> AnswerQueryAsync(string systemPrompt, string query, AiModel model = AiModel.GPT4_1)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            _logger.Warn("Query is null or empty. Returning empty response.");
            return String.Empty;
        }
        
        _logger.Debug($"Answering query '{query}' using model {model}...");
        
        // Build the request body
        var requestBody = new
        {
            model = GetModelStringFromEnum(model),
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = query }
            }
        };
        
        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        try
        {
            var response = await _httpClient.PostAsync($"{_config.OpenRouterBaseUrl}/chat/completions", content);
            response.EnsureSuccessStatusCode();
            
            string jsonResponse = await response.Content.ReadAsStringAsync();
            
            return ExtractTextFromResponse(jsonResponse);
        }
        catch (HttpRequestException ex)
        {
            // Gracefully handle specific HTTP request errors
            if (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.Warn("Request failed: Unauthorized. Please check your API key.");
                return "Request failed: Unauthorized. Please check your API key.";
            }
            
            throw new Exception($"Request failed: {ex.Message}", ex);
        }
    }
    
    
    /// <summary>
    /// Extracts the answer text from the AI response JSON (without all the metadata).
    /// </summary>
    /// <param name="responseJson">The JSON response from the AI service</param>
    /// <returns>The extracted text content from the response</returns>
    /// <exception cref="JsonException">Thrown if the response JSON cannot be parsed</exception>
    private string ExtractTextFromResponse(string responseJson)
    {
        try
        {
            var jsonObject = JObject.Parse(responseJson);
            
            // Navigate to choices[0].message.content
            var content = jsonObject["choices"]?[0]?["message"]?["content"]?.ToString();
            
            return content ?? "No content found";
        }
        catch (JsonException ex)
        {
            throw new Exception($"Failed to parse response: {ex.Message}", ex);
        }
    }
    
    
    /// <summary>
    /// Converts the <see cref="AiModel"/> enum to its corresponding string representation used by the OpenRouter API.
    /// </summary>
    /// <param name="model">The <see cref="AiModel"/> enum value to convert</param>
    /// <returns>The string representation of the AI model</returns>
    private string GetModelStringFromEnum(AiModel model)
    {
        return model switch
        {
            AiModel.GPT4_1 => "openai/gpt-4.1",
            _ => throw new ArgumentOutOfRangeException(nameof(model), $"Unsupported AI model: {model}")
        };
    }
    
    
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}