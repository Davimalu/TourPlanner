using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;

namespace TourPlanner.Logic
{
    public class SelectedTourService : ISelectedTourService
    {
        private Tour? _selectedTour;
        public Tour? SelectedTour {
            get => _selectedTour;
            set
            {
                _selectedTour = value;
                SelectedTourChanged?.Invoke(_selectedTour);
            }
        }

        public event Action<Tour?>? SelectedTourChanged;
    }
}