using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;

namespace TourPlanner.ViewModels
{
    class TourDetailsViewModel : BaseViewModel
    {
        private readonly ISelectedTourService _selectedTourService;

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


        public TourDetailsViewModel(ISelectedTourService selectedTourService)
        {
            _selectedTourService = selectedTourService ?? throw new ArgumentNullException(nameof(selectedTourService));
            
            _selectedTourService.SelectedTourChanged += (selectedTour) => SelectedTour = selectedTour; // Get the currently selected tour from the service
        }
    }
}
