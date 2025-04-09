using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourPlanner.Models;

namespace TourPlanner.DAL.Interfaces
{
    public interface ITourService
    {
        public Task<List<Tour>?> GetAllToursAsync();
        public Task<Tour?> GetTourByIdAsync(int id);
        public Task<Tour?> CreateTourAsync(Tour tour);
        public Task<Tour?> UpdateTourAsync(Tour tour);
        public Task<bool> DeleteTourAsync(int id);
    }
}
