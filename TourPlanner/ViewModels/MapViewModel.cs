using TourPlanner.Logic.Interfaces;
using TourPlanner.Models;

namespace TourPlanner.ViewModels
{
    class MapViewModel : BaseViewModel
    {
        private ISelectedTourService _selectedTourService;

        private Tour? _selectedTour;
        public Tour? SelectedTour
        {
            get { return _selectedTour; }
            set
            {
                _selectedTour = value;
                RaisePropertyChanged(nameof(SelectedTour));
            }
        }


        public MapViewModel(ISelectedTourService selectedTourService)
        {
            _selectedTourService = selectedTourService;
            _selectedTourService.SelectedTourChanged += (selectedTour) => SelectedTour = selectedTour; // Get the selected tour from the service
        }
    }
}
