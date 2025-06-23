using System.Windows;
using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Enums;
using TourPlanner.Model.Structs;
namespace TourPlanner.ViewModels
{
    class EditTourViewModel : BaseViewModel
    {
        // Dependencies
        private readonly ITourService _tourService;
        private readonly IOrsService _osrService;
        private readonly IMapService _mapService;
        private readonly ILoggerWrapper _logger;
        
        // Commands
        private RelayCommandAsync? _executeCalculateAndDrawRoute;
        private RelayCommandAsync? _executeSave;
        private RelayCommandAsync? _executeGeocodeStart;
        private RelayCommandAsync? _executeGeocodeEnd;
        
        public ICommand ExecuteCalculateAndDrawRoute => _executeCalculateAndDrawRoute ??= 
            new RelayCommandAsync(CalculateAndDrawRouteAsync, _ => GotStartCoordinates && GotEndCoordinates);
        
        public ICommand ExecuteSave => _executeSave ??= 
            new RelayCommandAsync(SaveAsync, _ => GotStartCoordinates && GotEndCoordinates && RouteCalculated);

        public ICommand ExecuteGeocodeStart => _executeGeocodeStart ??= 
            new RelayCommandAsync(GeocodeStartAsync);

        public ICommand ExecuteGeocodeEnd => _executeGeocodeEnd ??= 
            new RelayCommandAsync(GeocodeEndAsync);

        // Track whether we have valid start and end coordinates to enable / disable buttons accordingly
        private bool GotStartCoordinates => EditableTour.StartCoordinates != null;
        private bool GotEndCoordinates => EditableTour.EndCoordinates != null;

        private bool _routeCalculated = false;
        public bool RouteCalculated
        {
            get => _routeCalculated;
            set
            {
                _routeCalculated = value;
                RaisePropertyChanged(nameof(RouteCalculated));
            }
        }
        
        public List<Transport> Transports { get; set; }

        // Copy of the original Tour to edit (to avoid changing the original UNTIL the user saves)
        private Tour _editableTour = null!;
        public Tour EditableTour
        {
            get => _editableTour;
            set
            {
                _editableTour = value;
                RaisePropertyChanged(nameof(EditableTour));
            }
        }

        
        public EditTourViewModel(Tour selectedTour, ITourService tourService, IOrsService orsService, IMapService mapService, IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _tourService = tourService ?? throw new ArgumentNullException(nameof(tourService));
            _osrService = orsService ?? throw new ArgumentNullException(nameof(orsService));
            _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));

            _logger = LoggerFactory.GetLogger<EditTourViewModel>();

            EditableTour = new Tour(selectedTour); // Create a copy of the Tour to edit (so that if the user cancels, the original Tour remains unchanged)

            // Initialize enums (WPF can't bind to enums directly, so we use lists)
            Transports = new List<Transport>
            {
                Transport.Car, Transport.Bicycle, Transport.Walking, Transport.Wheelchair, Transport.ElectricBicycle,
                Transport.Mountainbike, Transport.Roadbike, Transport.Truck, Transport.Hiking
            };
        }
        
        
        private async Task CalculateAndDrawRouteAsync(object? parameter)
        {
            if (EditableTour.StartCoordinates == null || EditableTour.EndCoordinates == null)
            {
                _logger.Warn("Start or end coordinates are not set. Cannot calculate route.");
                return;
            }
            
            var routeInfo = await _osrService.GetRouteAsync(EditableTour.TransportationType, (GeoCoordinate)EditableTour.StartCoordinates, (GeoCoordinate)EditableTour.EndCoordinates);
            
            if (routeInfo == null)
            {
                _logger.Warn("Could not calculate route: Unable to get route information from OSR service.");
                return;
            }
            
            EditableTour.Distance = Math.Round(routeInfo.Distance / 1000, 2);
            EditableTour.EstimatedTime = (float)Math.Round(routeInfo.Duration / 60, 0);
            
            // Draw the route on the map
            await _mapService.ClearMapAsync(); // Clear existing markers and routes
            await _mapService.AddMarkerAsync(new MapMarker((GeoCoordinate)EditableTour.StartCoordinates, "Start", EditableTour.StartLocation));
            await _mapService.AddMarkerAsync(new MapMarker((GeoCoordinate)EditableTour.EndCoordinates, "End", EditableTour.EndLocation));
            await _mapService.DrawRouteAsync(routeInfo.RouteGeometry);
            
            EditableTour.GeoJsonString = routeInfo.RouteGeometry;
            
            RouteCalculated = true; // Mark that the route has been calculated
            _executeSave?.RaiseCanExecuteChanged(); // Notify the save command that the route has been calculated and thus it can be saved
        }


        private async Task SaveAsync(object? parameter)
        {
            // Check if the Tour already exists (i.e. are we updating an existing Tour or creating a new one?)

            // An invalid TourId means we are creating a new Tour
            if (EditableTour.TourId <= 0)
            {
                Tour? newTour = await _tourService.CreateTourAsync(EditableTour);

                if (newTour == null)
                {
                    _logger.Error($"Failed to create Tour: {EditableTour.TourName}");
                }
            }
            // Otherwise, we are updating an existing Tour
            else
            {
                Tour? updatedTour = await _tourService.UpdateTourAsync(EditableTour);

                if (updatedTour == null)
                {
                    _logger.Error($"Failed to update Tour with ID {EditableTour.TourId}: {EditableTour.TourName}");
                }
            }

            CloseWindow();
        }


        public ICommand ExecuteCancel => new RelayCommand(_ =>
        {
            // Close the window, discarding changes
            CloseWindow();
        });
        
        
        private async Task GeocodeStartAsync(object? parameter)
        {
            // Retrieve the geocoded coordinates for the start location
            GeoCode? coordinates = await _osrService.GetGeoCodeFromAddressAsync(EditableTour.StartLocation);

            if (coordinates == null)
            {
                _logger.Warn($"Could not find coordinates for address: {EditableTour.StartLocation}");
                return;
            }
            
            // Update the tour's fields with the geocoded coordinates
            EditableTour.StartCoordinates = coordinates.Coordinates;
            EditableTour.StartLocation = coordinates.Label;
            
            // Remove existing start marker from the map
            await _mapService.RemoveMarkerByTitleAsync("Start");
            
            // Add new marker on the map for the start location
            await _mapService.AddMarkerAsync(new MapMarker(coordinates.Coordinates, "Start", coordinates.Label));
            await _mapService.SetViewToCoordinatesAsync(coordinates.Coordinates);
            
            // Notify the draw route command that the start coordinates have changed (and thus the route might be able to be calculated)
            _executeCalculateAndDrawRoute?.RaiseCanExecuteChanged();
        }
        
        
        private async Task GeocodeEndAsync(object? parameter)
        {
            // Retrieve the geocoded coordinates for the end location
            GeoCode? coordinates = await _osrService.GetGeoCodeFromAddressAsync(EditableTour.EndLocation);
            
            if (coordinates == null)
            {
                _logger.Warn($"Could not find coordinates for address: {EditableTour.EndLocation}");
                return;
            }
            
            // Update the tour's fields with the geocoded coordinates
            EditableTour.EndCoordinates = coordinates.Coordinates;
            EditableTour.EndLocation = coordinates.Label;
            
            // Remove existing end marker from the map
            await _mapService.RemoveMarkerByTitleAsync("End");
            
            // Add new marker on the map for the end location
            await _mapService.AddMarkerAsync(new MapMarker(coordinates.Coordinates, "End", coordinates.Label));
            await _mapService.SetViewToCoordinatesAsync(coordinates.Coordinates);
            
            // Notify the draw route command that the end coordinates have changed (and thus the route might be able to be calculated)
            _executeCalculateAndDrawRoute?.RaiseCanExecuteChanged();
        }


        private void CloseWindow()
        {
            // Make the MapService control the map in the main window again
            _mapService.SwitchControlToMainMapAsync();
            
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}