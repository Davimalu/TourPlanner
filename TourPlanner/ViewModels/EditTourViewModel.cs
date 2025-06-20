using System.Windows;
using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.DAL.ServiceAgents;
using TourPlanner.Enums;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Enums;

namespace TourPlanner.ViewModels
{
    class EditTourViewModel : BaseViewModel
    {
        private readonly ITourService _tourService;
        private readonly IOrsService _iosrService;
        private readonly ILoggerWrapper _logger;
        
        public ICommand FindStartLocationCommand { get; }
        public ICommand FindEndLocationCommand { get; }


        // Copy of the original Tour to edit (to avoid changing the original UNTIL the user saves)
        private Tour _editableTour = null!;
        public Tour EditableTour
        {
            get => _editableTour;
            set { _editableTour = value; RaisePropertyChanged(nameof(EditableTour)); }
        }
        
        public List<Transport> Transports { get; set; }
        
        
        
        
        public MapViewModel MapViewModel { get; }

        private (double lon, double lat)? _startPoint;
        private (double lon, double lat)? _endPoint;

        public EditTourViewModel(Tour selectedTour, ITourService tourService, IOrsService iosrService)
        {
            _tourService = tourService ?? throw new ArgumentNullException(nameof(tourService));
            _iosrService = iosrService ?? throw new ArgumentNullException(nameof(iosrService));
            
            _logger = LoggerFactory.GetLogger<EditTourViewModel>();
            
            EditableTour = new Tour(selectedTour); // Create a copy of the Tour to edit (so that if the user cancels, the original Tour remains unchanged)
            
            
            
            MapViewModel = new MapViewModel(null);
            System.Diagnostics.Debug.WriteLine("EditTourViewModel: Subscribing to MapClicked event.");
            MapViewModel.MapClicked += OnMapClicked;
            
            EditableTour.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Tour.TransportationType))
                {
                    CalculateAndDrawRoute();
                }
            };
            
            FindStartLocationCommand = new RelayCommandAsync(async _ => await GeocodeAddress(true));
            FindEndLocationCommand = new RelayCommandAsync(async _ => await GeocodeAddress(false));
            
            
            
            
            // Initialize enums (WPF can't bind to enums directly, so we use lists)
            Transports = new List<Transport> { Transport.Car, Transport.Bicycle, Transport.Walking, Transport.Motorcycle };
        }
        
        private async Task GeocodeAddress(bool isStart)
        {
            string address = isStart ? EditableTour.StartLocation : EditableTour.EndLocation;
            if (string.IsNullOrWhiteSpace(address)) return;

            var coordinates = await _iosrService.GetCoordinatesFromAddressAsync(address);
            if (coordinates.HasValue)
            {
                var (lon, lat) = coordinates.Value;
                if (isStart)
                {
                    // Simulate a map click with the new coordinates
                    OnMapClicked(this, new MapClickEventArgs(lat, lon));
                }
                else
                {
                    if (_startPoint.HasValue) // Ensure start is set before setting end
                    {
                        OnMapClicked(this, new MapClickEventArgs(lat, lon));
                    }
                    else
                    {
                        // Handle error: user must set start location first
                        _logger.Warn("Please set the start location before the end location.");
                    }
                }
            }
            else
            {
                _logger.Error($"Could not find coordinates for address: {address}");
            }
        }

        private void OnMapClicked(object? sender, MapClickEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"EditTourViewModel: OnMapClicked received! Lat: {e.Lat}, Lon: {e.Lon}");
            
            if (_startPoint == null)
            {
                _startPoint = (e.Lon, e.Lat);
                // FIX: Save coordinates to the model
                EditableTour.StartLat = e.Lat;
                EditableTour.StartLon = e.Lon;

                MapViewModel.ClearMap();
                MapViewModel.AddMarker(e.Lat, e.Lon, "start");
                EditableTour.StartLocation = $"Coord: {e.Lat:F5}, {e.Lon:F5}";
            }
            else if (_endPoint == null)
            {
                _endPoint = (e.Lon, e.Lat);
                // FIX: Save coordinates to the model
                EditableTour.EndLat = e.Lat;
                EditableTour.EndLon = e.Lon;

                MapViewModel.AddMarker(e.Lat, e.Lon, "end");
                EditableTour.EndLocation = $"Coord: {e.Lat:F5}, {e.Lon:F5}";
                CalculateAndDrawRoute();
            }
            else
            {
                _startPoint = (e.Lon, e.Lat);
                _endPoint = null;

                // FIX: Save coordinates to the model and reset end point
                EditableTour.StartLat = e.Lat;
                EditableTour.StartLon = e.Lon;
                EditableTour.EndLat = 0;
                EditableTour.EndLon = 0;

                MapViewModel.ClearMap();
                MapViewModel.AddMarker(e.Lat, e.Lon, "start");
                EditableTour.StartLocation = $"Coord: {e.Lat:F5}, {e.Lon:F5}";
                EditableTour.EndLocation = "";
                EditableTour.Distance = 0;
                EditableTour.EstimatedTime = 0;
            }
        }
        
        private async void CalculateAndDrawRoute()
        {
            if (_startPoint == null || _endPoint == null) return;

            var routeInfo = await _iosrService.GetRouteAsync(EditableTour.TransportationType, _startPoint.Value, _endPoint.Value);

            if (routeInfo != null)
            {
                EditableTour.Distance = Math.Round(routeInfo.Distance / 1000, 2);
                EditableTour.EstimatedTime = (float)Math.Round(routeInfo.Duration / 60, 0);
                MapViewModel.DrawRoute(routeInfo.RouteGeometry);
            }
            else
            {
                _logger.Warn("Could not calculate route from MapService.");
            }
        }

        
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