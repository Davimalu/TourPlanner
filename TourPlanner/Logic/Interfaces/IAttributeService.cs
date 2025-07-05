using TourPlanner.Model;

namespace TourPlanner.Logic.Interfaces;

public interface IAttributeService
{
    Task<float> CalculatePopularityAsync(Tour tour);
    float CalculateChildFriendliness(Tour tour);
    Task<string> GetAiSummaryAsync(Tour tour);
}