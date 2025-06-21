using TourPlanner.Model;

namespace TourPlanner.Logic.Interfaces;

public interface ISearchService
{
    Task<List<Tour>> SearchToursAsync(string query, List<Tour> tours);
}