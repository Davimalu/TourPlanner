using Microsoft.Extensions.Configuration;
using TourPlanner.config.Interfaces;

namespace TourPlanner.config;

public class TourPlannerConfig : ITourPlannerConfig
{
    private readonly IConfiguration _configuration;
    
    public TourPlannerConfig()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("config/appsettings.json", optional: true, reloadOnChange: true)
            .Build();
    }

    public string OpenRouteServiceBaseUrl => _configuration["OpenRouteService:BaseUrl"] 
                                            ?? throw new InvalidOperationException("OpenRouteService Base URL is not configured.");

    public string OpenRouteServiceApiKey => _configuration["OpenRouteService:ApiKey"] 
                                            ?? throw new InvalidOperationException("OpenRouteService API key is not configured.");
    
    public string ApiBaseUrl => _configuration["ApiSettings:BaseUrl"]
                                    ?? throw new InvalidOperationException("API Base URL is not configured.");
}