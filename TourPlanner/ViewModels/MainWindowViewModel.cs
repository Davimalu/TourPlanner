using System.Diagnostics;
using TourPlanner.Models;

namespace TourPlanner.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly TourListViewModel _tourList;

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


        public MainWindowViewModel(TourListViewModel tourList)
        {
            _tourList = tourList;
            tourList.SelectedTourChanged += (selectedTour) => SelectedTour = selectedTour;
        }
    }
}
