using TourPlanner.Model;
using TourPlanner.Models;

namespace TourPlanner.Logic.Interfaces
{
    internal interface IWindowService
    {
        public void SpawnEditTourWindow(Tour selectedTour);
        public void SpawnEditTourLogWindow (Tour selectedTour, TourLog selectedTourLog);
    }
}
