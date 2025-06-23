using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Events;
using TourPlanner.Model.Structs;

namespace TourPlanner.ViewModels
{
    public class MapViewModel : BaseViewModel
    {
        private readonly IMapService _mapService;
        private readonly ILoggerWrapper _logger;
        
        private Tour? _selectedTour;
        public Tour? SelectedTour
        {
            get => _selectedTour;
            set
            {
                if (_selectedTour != value)
                {
                    _selectedTour = value;
                    RaisePropertyChanged(nameof(SelectedTour));
                }
            }
        }
        
        
        public MapViewModel(IMapService mapService, IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
            _logger = LoggerFactory.GetLogger<MapViewModel>();

            EventAggregator.Subscribe<SelectedTourChangedEvent>(OnSelectedTourChanged);
        }
        
        
        /// <summary>
        /// Handles changes to the selected tour and updates the map accordingly
        /// </summary>
        /// <param name="e"><see cref="SelectedTourChangedEvent"/> containing the new selected tour</param>
        private async void OnSelectedTourChanged(SelectedTourChangedEvent e)
        {
            // Update the internal selected tour property
            SelectedTour = e.SelectedTour;
            
            if (SelectedTour != null)
            {
                // Redraw the route on the map if a new tour is selected
                _logger.Debug($"Selected tour changed: {SelectedTour.TourName} (ID: {SelectedTour.TourId}). Redrawing route on map...");
                await DisplayTourRouteAsync(SelectedTour);
            }
            else
            {
                // If no tour is selected, clear the map
                await _mapService.ClearMapAsync();
            }
        }
        
        
        /// <summary>
        /// Displays the route for the specified tour on the map
        /// </summary>
        /// <param name="tour">The tour whose route to display</param>
        private async Task DisplayTourRouteAsync(Tour tour)
        {
            if (tour.StartCoordinates == null || tour.EndCoordinates == null)
            {
                _logger.Warn($"Tour '{tour.TourName}' does not have valid start or end coordinates.");
                await _mapService.ClearMapAsync();
                return;
            }
            
            try
            {
                _logger.Debug($"Displaying route for tour '{tour.TourName}'");
                
                await _mapService.ClearMapAsync();
                await _mapService.AddMarkerAsync(new MapMarker((GeoCoordinate)tour.StartCoordinates, "Start", tour.StartLocation));
                await _mapService.AddMarkerAsync(new MapMarker((GeoCoordinate)tour.EndCoordinates, "End", tour.EndLocation));
                await _mapService.DrawRouteAsync(tour.GeoJsonString);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to display tour route: {ex.Message}", ex);
                await _mapService.ClearMapAsync();
            }
        }
    }
}