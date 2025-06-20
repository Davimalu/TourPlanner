using System.Windows;
using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Enums;
using TourPlanner.Model.Structs;

namespace TourPlanner.ViewModels
{
    class EditTourViewModel : BaseViewModel
    {
        private readonly MapViewModel _mapViewModel; // TODO: Is it okay for one ViewModel to depend on another ViewModel?
        private readonly ITourService _tourService;
        private readonly IOrsService _osrService;
        private readonly ILoggerWrapper _logger;
        
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

        
        public EditTourViewModel(MapViewModel mapViewModel, Tour selectedTour, ITourService tourService, IOrsService orsService)
        {
            _mapViewModel = mapViewModel ?? throw new ArgumentNullException(nameof(mapViewModel));
            _tourService = tourService ?? throw new ArgumentNullException(nameof(tourService));
            _osrService = orsService ?? throw new ArgumentNullException(nameof(orsService));

            _logger = LoggerFactory.GetLogger<EditTourViewModel>();

            EditableTour = new Tour(selectedTour); // Create a copy of the Tour to edit (so that if the user cancels, the original Tour remains unchanged)

            InitializeMap();
            
            // Initialize enums (WPF can't bind to enums directly, so we use lists)
            Transports = new List<Transport>
            {
                Transport.Car, Transport.Bicycle, Transport.Walking, Transport.Wheelchair, Transport.ElectricBicycle,
                Transport.Mountainbike, Transport.Roadbike, Transport.Truck, Transport.Hiking
            };
        }


        private async void InitializeMap()
        {
            try
            {
                _mapViewModel.MapClicked += OnMapClicked; // Subscribe to the MapClicked event
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to initialize MapViewModel: {ex.Message}");
                Application.Current.Shutdown();
            }
        }
        

        private void OnMapClicked(object? sender, GeoCoordinate e)
        {
            _logger.Debug($"OnMapClicked Event received! Lat: {e.Latitude}, Lon: {e.Longitude}");
            
            if (EditableTour.StartCoordinates == null)
            {
                EditableTour.StartCoordinates = e;
                EditableTour.StartLocation = $"Coord: {e.Latitude:F5}, {e.Longitude:F5}";
            }
            else if (EditableTour.EndCoordinates == null)
            {
                EditableTour.EndCoordinates = e;
                EditableTour.EndLocation = $"Coord: {e.Latitude:F5}, {e.Longitude:F5}";
            }
            else
            {
                // Reset start and end coordinates if both are already set
                EditableTour.StartCoordinates = e;
                EditableTour.EndCoordinates = null;
                EditableTour.StartLocation = $"Coord: {e.Latitude:F5}, {e.Longitude:F5}";
                EditableTour.EndLocation = "";
            }
        }
        
        
        public ICommand ExecuteCalculateAndDrawRoute => new RelayCommandAsync(async _ =>
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
            
            // TODO: Draw the route on the map
        });


        public ICommand ExecuteSave => new RelayCommandAsync(async _ =>
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
        });


        public ICommand ExecuteCancel => new RelayCommand(_ =>
        {
            // Close the window, discarding changes
            CloseWindow();
        });
        
        
        public ICommand ExecuteGeocodeStart => new RelayCommandAsync(async _ =>
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
        });
        
        
        public ICommand ExecuteGeocodeEnd => new RelayCommandAsync(async _ =>
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
        });


        private void CloseWindow()
        {
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