namespace TourPlanner.config.Interfaces;

public interface ITourPlannerConfig
{
    string OpenRouteServiceBaseUrl { get; }
    string OpenRouteServiceApiKey { get; }
    string ApiBaseUrl { get; }
    string OpenRouterBaseUrl { get; }
    string OpenRouterApiKey { get; }
    string TmpFolder { get;  }
}