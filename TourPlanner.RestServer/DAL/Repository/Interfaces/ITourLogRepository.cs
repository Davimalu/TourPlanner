using TourPlanner.Model;

namespace TourPlanner.RestServer.DAL.Repository.Interfaces;

public interface ITourLogRepository
{
    Task<TourLog> GetTourLogByIdAsync(int id);
    Task<TourLog> AddTourLogAsync(int parentTourId, TourLog newLog);
    Task<TourLog> UpdateTourLogAsync(TourLog updatedTourLog);
    Task<bool> DeleteTourLogAsync(int id);
}