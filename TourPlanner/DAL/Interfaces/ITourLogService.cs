using TourPlanner.Model;
using TourPlanner.Models;

namespace TourPlanner.DAL.Interfaces
{
    public interface ITourLogService
    {
        public Task<List<TourLog>> GetTourLogsAsync(int tourId);
        public Task<TourLog?> GetTourLogByIdAsync(int logId);
        public Task<TourLog?> CreateTourLogAsync(int tourId, TourLog tourLog);
        public Task<TourLog?> UpdateTourLogAsync(TourLog tourLog);
        public Task<bool> DeleteTourLogAsync(int logId);
    }
}
