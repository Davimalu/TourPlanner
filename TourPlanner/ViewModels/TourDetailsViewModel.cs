using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Events;

namespace TourPlanner.ViewModels
{
    public class TourDetailsViewModel : BaseViewModel
    {
        private Tour? _selectedTour;
        public Tour? SelectedTour
        {
            get => _selectedTour;
            set
            {
                _selectedTour = value;
                RaisePropertyChanged(nameof(SelectedTour));
            }
        }


        public TourDetailsViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
            // Subscribe to changes in the selected tour so that we can display its details
            EventAggregator.Subscribe<SelectedTourChangedEvent>(OnSelectedTourChanged);
        }


        /// <summary>
        /// Handles the SelectedTourChangedEvent to update the classes own SelectedTour property.
        /// </summary>
        /// <param name="e">The event containing the newly selected tour</param>
        private void OnSelectedTourChanged(SelectedTourChangedEvent e)
        {
            SelectedTour = e.SelectedTour;
        }
    }
}
