using TourPlanner.Logic.Interfaces;
using TourPlanner.Model.Events;

namespace TourPlanner.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public bool InfoTabSelected { get; set; } = false;
        public bool MapTabSelected { get; set; } = true;
        public bool MiscTabSelected { get; set; } = false;
        
        public MainWindowViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
            EventAggregator.Subscribe<MapScreenshotRequestedEvent>(OnMapScreenshotRequested);
        }


        /// <summary>
        /// Switches to the Map tab when a screenshot is requested - otherwise the screenshot can't be taken correctly
        /// </summary>
        /// <param name="event">The event containing the screenshot request</param>
        private async void OnMapScreenshotRequested(MapScreenshotRequestedEvent @event)
        {
            // Save the current state of the tabs
            bool previousInfoTabSelected = InfoTabSelected;
            bool previousMapTabSelected = MapTabSelected;
            bool previousMiscTabSelected = MiscTabSelected;
            
            // Switch to the Map tab
            InfoTabSelected = false;
            MiscTabSelected = false;
            MapTabSelected = true;
            
            // Raise property changed events to update the UI
            RaisePropertyChanged(nameof(InfoTabSelected));
            RaisePropertyChanged(nameof(MapTabSelected));
            RaisePropertyChanged(nameof(MiscTabSelected));
        }
    }
}
