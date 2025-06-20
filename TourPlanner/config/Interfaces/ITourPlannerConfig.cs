namespace TourPlanner.config.Interfaces;

public interface ITourPlannerConfig
{
    string OpenRouteServiceBaseUrl { get; }
    string OpenRouteServiceApiKey { get; }
    string ApiBaseUrl { get; }
}