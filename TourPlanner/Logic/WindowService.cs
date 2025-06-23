using TourPlanner.DAL.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.ViewModels;
using TourPlanner.Views;

namespace TourPlanner.Logic
{
    internal class WindowService : IWindowService
    {
        private readonly ITourLogService _tourLogService;
        private readonly ITourService _tourService;
        private readonly IOrsService _osrService;
        private readonly IMapService _mapService;
        private readonly IAttributeService _attributeService;
        private readonly IEventAggregator _eventAggregator;
        
        private readonly MapViewModel _mapViewModel;
        
        public WindowService(ITourLogService tourLogService, ITourService tourService, IOrsService iosrService, IMapService mapService, MapViewModel mapViewModel, IAttributeService attributeService, IEventAggregator eventAggregator)
        {
            _tourLogService = tourLogService ?? throw new ArgumentNullException(nameof(tourLogService));
            _tourService = tourService ?? throw new ArgumentNullException(nameof(tourService));
            _osrService = iosrService ?? throw new ArgumentNullException(nameof(iosrService));
            _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
            _mapViewModel = mapViewModel ?? throw new ArgumentNullException(nameof(mapViewModel));
            _attributeService = attributeService ?? throw new ArgumentNullException(nameof(attributeService));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }
        
        
        public void SpawnEditTourWindow(Tour selectedTour)
        {
            var editWindow = new EditTourWindow()
            {
                DataContext = new EditTourViewModel(selectedTour, _tourService, _osrService, _mapService, _eventAggregator),
                Map = { DataContext = _mapViewModel}
            };

            editWindow.ShowDialog();
        }


        public void SpawnEditTourLogWindow(Tour selectedTour, TourLog selectedTourLog)
        {
            var editWindow = new EditTourLogWindow
            {
                DataContext = new EditTourLogViewModel(selectedTour, _tourService, selectedTourLog, _tourLogService, _attributeService, _eventAggregator),
            };

            editWindow.ShowDialog();
        }
    }
}
