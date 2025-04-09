using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourPlanner.Models;
using TourPlanner.Views;

namespace TourPlanner.DAL.Interfaces
{
    public interface ITourLogService
    {
        public Task<List<TourLog>?> GetTourLogsAsync(int tourId);
        public Task<TourLog?> GetTourLogByIdAsync(int logId);
        public Task<TourLog?> CreateTourLogAsync(int tourId, TourLog tourLog);
        public Task<TourLog?> UpdateTourLogAsync(TourLog tourLog);
        public Task<bool> DeleteTourLogAsync(int logId);
    }
}
