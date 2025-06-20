using TourPlanner.Model;

namespace TourPlanner.RestServer.DAL.Repository.Interfaces;

public interface ITourRepository
{
    Task<IEnumerable<Tour>> GetAllToursAsync();
    Task<Tour> GetTourByIdAsync(int id);
    Task<Tour> AddTourAsync(Tour tour);
    Task<Tour> UpdateTourAsync(Tour tour);
    Task<bool> DeleteTourAsync(int id);
}