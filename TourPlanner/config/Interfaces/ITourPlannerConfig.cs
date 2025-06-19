namespace TourPlanner.config.Interfaces;

public interface ITourPlannerConfig
{
    string OpenRouteServiceApiKey { get; }
    string ApiBaseUrl { get; }
}