using TourPlanner.Models;

namespace TourPlanner.ViewModels
{
    class TourDetailsViewModel : BaseViewModel
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


        public TourDetailsViewModel(TourListViewModel tourListViewModel)
        {
            tourListViewModel.SelectedTourChanged += (selectedTour) => SelectedTour = selectedTour; // Get the selected tour from the TourListViewModel
        }
    }
}
