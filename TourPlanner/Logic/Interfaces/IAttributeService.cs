using TourPlanner.Model;

namespace TourPlanner.Logic.Interfaces;

public interface IAttributeService
{
    public Task<float> CalculatePopularityAsync(Tour tour);
    public float CalulateChildFriendliness(Tour tour);
}