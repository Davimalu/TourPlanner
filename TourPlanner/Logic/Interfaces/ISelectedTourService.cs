using TourPlanner.Models;

namespace TourPlanner.Logic.Interfaces
{
    public interface ISelectedTourService
    {
        Tour? SelectedTour { get; set; }
        event Action<Tour?> SelectedTourChanged;
    }
}
