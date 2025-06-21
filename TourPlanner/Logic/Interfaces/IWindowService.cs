using TourPlanner.Model;

namespace TourPlanner.Logic.Interfaces
{
    public interface IWindowService
    {
        public void SpawnEditTourWindow(Tour selectedTour);
        public void SpawnEditTourLogWindow (Tour selectedTour, TourLog selectedTourLog);
    }
}
