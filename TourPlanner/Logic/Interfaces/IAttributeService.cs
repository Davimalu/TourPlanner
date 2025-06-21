using TourPlanner.Model;

namespace TourPlanner.Logic.Interfaces;

public interface IAttributeService
{
    public float CalculatePopularity(Tour tour);
    public float CalulateChildFriendliness(Tour tour);
}