using TourPlanner.Model;

namespace TourPlanner.DAL.Interfaces;

public interface ILocalTourService
{
    Task<bool> SaveToursToFileAsync(IEnumerable<Tour> tours, string path);
    Task<IEnumerable<Tour>?> LoadToursFromFileAsync(string path);
}