using TourPlanner.Models;

namespace TourPlanner.ViewModels
{
    class MapViewModel : BaseViewModel
    {
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


        public MapViewModel(TourListViewModel tourListViewModel)
        {
            tourListViewModel.SelectedTourChanged += (selectedTour) => SelectedTour = selectedTour; // Get the selected tour from the TourListViewModel
        }
    }
}
