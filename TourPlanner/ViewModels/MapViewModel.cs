using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Structs;

namespace TourPlanner.ViewModels
{
    public class MapViewModel : BaseViewModel
    {
        private readonly ISelectedTourService? _selectedTourService;
        private readonly IMapService _mapService;
        private readonly IOrsService _orsService;
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

        public event EventHandler<GeoCoordinate>? MapClicked;

        
        public MapViewModel(ISelectedTourService selectedTourService, IMapService mapService, IOrsService orsService)
        {
            _selectedTourService = selectedTourService ?? throw new ArgumentNullException(nameof(selectedTourService));
            _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
            _orsService = orsService ?? throw new ArgumentNullException(nameof(orsService));
            _logger = LoggerFactory.GetLogger<MapViewModel>();
            
            _selectedTourService.SelectedTourChanged += (selectedTour) => SelectedTour = selectedTour; // Get the currently selected tour from the service
            
            _mapService.MapClicked += OnMapClicked; // Subscribe to the MapClicked event from the MapService
            _selectedTourService.SelectedTourChanged += OnSelectedTourChanged; // Subscribe to changes in the selected tour
        }
        
        
        /// <summary>
        /// Initializes the MapViewModel by ensuring the Map is ready and displaying the selected tour's route if available
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                // If a tour was already selected before the initialization, display its route
                if (_selectedTourService?.SelectedTour != null)
                {
                    await DisplayTourRouteAsync(_selectedTourService.SelectedTour);
                }
                
                _logger.Info("MapViewModel initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to initialize MapViewModel: {ex.Message}", ex);
            }
        }
        
        
        /// <summary>
        /// Handles changes to the selected tour and updates the map accordingly
        /// </summary>
        /// <param name="selectedTour">The newly selected tour</param>
        private async void OnSelectedTourChanged(Tour? selectedTour)
        {
            SelectedTour = selectedTour;
            
            if (selectedTour != null)
            {
                _logger.Debug($"Selected tour changed: {selectedTour.TourName} (ID: {selectedTour.TourId}). Redrawing route on map...");
                await DisplayTourRouteAsync(selectedTour);
            }
            else
            {
                await _mapService.ClearMapAsync();
            }
        }
        
        
        /// <summary>
        /// Handles the MapClicked event from the MapService and forwards it to any subscribers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapClicked(object? sender, GeoCoordinate e)
        {
            MapClicked?.Invoke(this, e);
            _logger.Debug($"Map clicked event forwarded: ({e.Latitude}, {e.Longitude})");
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

                var route = await _orsService.GetRouteAsync(tour.TransportationType, (GeoCoordinate)tour.StartCoordinates, (GeoCoordinate)tour.EndCoordinates);

                if (route?.RouteGeometry != null)
                {
                    await _mapService.ClearMapAsync();
                    await _mapService.AddMarkerAsync(new MapMarker((GeoCoordinate)tour.StartCoordinates, "Start", tour.StartLocation));
                    await _mapService.AddMarkerAsync(new MapMarker((GeoCoordinate)tour.EndCoordinates, "End", tour.EndLocation));
                    await _mapService.DrawRouteAsync(route.RouteGeometry);
                }
                else
                {
                    _logger.Warn($"Failed to get route for tour '{tour.TourName}'");
                    await _mapService.ClearMapAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to display tour route: {ex.Message}", ex);
                await _mapService.ClearMapAsync();
            }
        }
    }
}