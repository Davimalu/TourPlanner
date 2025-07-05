using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Enums;
using TourPlanner.Model.Events;
using TourPlanner.Model.Structs;
using MessageBoxButton = TourPlanner.Model.Enums.MessageBoxAbstraction.MessageBoxButton;
using MessageBoxImage = TourPlanner.Model.Enums.MessageBoxAbstraction.MessageBoxImage;

namespace TourPlanner.ViewModels
{
    public class EditTourViewModel : BaseViewModel
    {
        // Dependencies
        private readonly ITourService _tourService;
        private readonly IOrsService _orsService;
        private readonly IMapService _mapService;
        private readonly IWpfService _wpfService;
        private readonly ILogger<EditTourViewModel> _logger;
        
        // Commands
        private RelayCommandAsync? _executeCalculateAndDrawRoute;
        private RelayCommandAsync? _executeSave;
        private RelayCommandAsync? _executeGeocodeStart;
        private RelayCommandAsync? _executeGeocodeEnd;
        private RelayCommand? _executeCancel;
        
        public ICommand ExecuteCalculateAndDrawRoute => _executeCalculateAndDrawRoute ??= 
            new RelayCommandAsync(CalculateAndDrawRouteAsync, _ => GotStartCoordinates && GotEndCoordinates);
        
        public ICommand ExecuteSave => _executeSave ??= 
            new RelayCommandAsync(SaveAsync, _ => GotStartCoordinates && GotEndCoordinates && RouteCalculated && !string.IsNullOrWhiteSpace(TourName));

        public ICommand ExecuteGeocodeStart => _executeGeocodeStart ??= 
            new RelayCommandAsync(param => GeocodeLocationAsync(param, true));

        public ICommand ExecuteGeocodeEnd => _executeGeocodeEnd ??= 
            new RelayCommandAsync(param => GeocodeLocationAsync(param, false));
        
        public ICommand ExecuteCancel => _executeCancel ??= 
            new RelayCommand(CancelEditTour);

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
        
        // WPF can't bind to enums directly, so we use lists
        public List<Transport> Transports { get; set; }
        
        // The user will edit this Tour while this window is open
        // If the user cancels, the original Tour remains unchanged | If the user saves, the original Tour is overwritten with this one
        private Tour? _editableTour;
        public Tour EditableTour
        {
            get => _editableTour ?? new Tour();
            set
            {
                _editableTour = value;
                RaisePropertyChanged(nameof(EditableTour));
            }
        }
        
        
        public string TourName
        {
            get => EditableTour.TourName;
            set
            {
                EditableTour.TourName = value;
                RaisePropertyChanged(nameof(EditableTour));
                
                // Notify the save command that the state may have changed
                _executeSave?.RaiseCanExecuteChanged();
            }
        }
        
        
        public string StartLocation
        {
            get => EditableTour.StartLocation;
            set
            {
                EditableTour.StartLocation = value;
                RaisePropertyChanged(nameof(EditableTour));
                
                // If the start location changes, the user needs to find the coordinates and calculate the route again
                ResetRouteCalculatedState(true);
            }
        }
        
        
        public string EndLocation
        {
            get => EditableTour.EndLocation;
            set
            {
                EditableTour.EndLocation = value;
                RaisePropertyChanged(nameof(EditableTour));
                
                // If the end location changes, the user needs to find the coordinates and calculate the route again
                ResetRouteCalculatedState(false);
            }
        }

        
        public EditTourViewModel(Tour selectedTour, ITourService tourService, IOrsService orsService, IMapService mapService, IWpfService wpfService, IEventAggregator eventAggregator, ILogger<EditTourViewModel> logger) : base(eventAggregator)
        {
            _tourService = tourService ?? throw new ArgumentNullException(nameof(tourService));
            _orsService = orsService ?? throw new ArgumentNullException(nameof(orsService));
            _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
            _wpfService = wpfService ?? throw new ArgumentNullException(nameof(wpfService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            EditableTour = new Tour(selectedTour); // Create a copy of the Tour to edit (so that if the user cancels, the original Tour remains unchanged)

            // Initialize enums (WPF can't bind to enums directly, so we use lists)
            Transports = new List<Transport>
            {
                Transport.Car, Transport.Bicycle, Transport.Walking, Transport.Wheelchair, Transport.ElectricBicycle,
                Transport.Mountainbike, Transport.Roadbike, Transport.Truck, Transport.Hiking
            };
        }
        
        
        /// <summary>
        /// Calculates the route based on the start and end coordinates of the SelectedTour and draws it on the map
        /// </summary>
        /// <param name="parameter"></param>
        private async Task CalculateAndDrawRouteAsync(object? parameter)
        {
            if (EditableTour.StartCoordinates == null || EditableTour.EndCoordinates == null)
            {
                _logger.Warn("Start or end coordinates are not set. Cannot calculate route.");
                return;
            }
            
            var routeInfo = await _orsService.GetRouteAsync(EditableTour.TransportationType, (GeoCoordinate)EditableTour.StartCoordinates, (GeoCoordinate)EditableTour.EndCoordinates);
            
            if (routeInfo == null)
            {
                _logger.Warn("Could not calculate route: Unable to get route information from OSR service.");
                _wpfService.ShowMessageBox("Route Calculation Error", "Could not calculate the route. Please check your start and end locations.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            EditableTour.Distance = Math.Round(routeInfo.Distance / 1000, 2);
            EditableTour.EstimatedTime = TimeSpan.FromSeconds(routeInfo.Duration);
            
            // Draw the route on the map
            await _mapService.ClearMapAsync(); // Clear existing markers and routes
            await _mapService.AddMarkerAsync(new MapMarker((GeoCoordinate)EditableTour.StartCoordinates, "Start", EditableTour.StartLocation));
            await _mapService.AddMarkerAsync(new MapMarker((GeoCoordinate)EditableTour.EndCoordinates, "End", EditableTour.EndLocation));
            await _mapService.DrawRouteAsync(routeInfo.RouteGeometry);
            
            EditableTour.GeoJsonString = routeInfo.RouteGeometry;
            
            RouteCalculated = true; // Mark that the route has been calculated
            _executeSave?.RaiseCanExecuteChanged(); // Notify the save command that the route has been calculated and thus it can be saved
        }


        /// <summary>
        /// Creates or updates the Tour to the database and closes the window
        /// </summary>
        /// <param name="parameter"></param>
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


        /// <summary>
        /// Cancels the edit operation and closes the window without saving changes
        /// </summary>
        /// <param name="parameter"></param>
        private void CancelEditTour(object? parameter)
        {
            // Close the window, discarding changes
            CloseWindow();
        }
        
        
        /// <summary>
        /// Geocodes the start location of the Tour and adds a marker on the map
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="isStartLocation">Indicates whether the start or end location should be geocoded</param>
        private async Task GeocodeLocationAsync(object? parameter, bool isStartLocation)
        {
            // Retrieve the geocoded coordinates for the start location
            GeoCode? coordinates = await _orsService.GetGeoCodeFromAddressAsync(isStartLocation ? EditableTour.StartLocation : EditableTour.EndLocation);

            if (coordinates == null)
            {
                _logger.Warn("Could not find coordinates for address: " + (isStartLocation ? EditableTour.StartLocation : EditableTour.EndLocation));
                _wpfService.ShowMessageBox("Geocoding Error", "Could not find coordinates for the specified address. Please check the address and try again.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Update the tour's fields with the geocoded coordinates
            if (isStartLocation)
            {
                EditableTour.StartCoordinates = coordinates.Coordinates;
                EditableTour.StartLocation = coordinates.Label;
                
                RaisePropertyChanged(nameof(StartLocation));
            }
            else
            {
                EditableTour.EndCoordinates = coordinates.Coordinates;
                EditableTour.EndLocation = coordinates.Label;
                
                RaisePropertyChanged(nameof(EndLocation));
            }
            
            string markerTitle = isStartLocation ? "Start" : "End";
            
            // Remove existing marker from the map
            await _mapService.RemoveMarkerByTitleAsync(markerTitle);
            
            // Add new marker on the map for the start location
            await _mapService.AddMarkerAsync(new MapMarker(coordinates.Coordinates, markerTitle, coordinates.Label));
            await _mapService.SetViewToCoordinatesAsync(coordinates.Coordinates);
            
            // Notify the draw route command that the start coordinates have changed (and thus the route might be able to be calculated)
            _executeCalculateAndDrawRoute?.RaiseCanExecuteChanged();
            
            // The user could click the "Find" Button without editing the text box and OpenRouteServices might return a different location than before
            // Thus we reset the route calculated state to force the user to recalculate the route (even if he didn't explictly change the location but only geocoded it)
            RouteCalculated = false;
            _executeSave?.RaiseCanExecuteChanged();
        }
        
        
        /// <summary>
        /// Closes the current window and switches control back to the main map in the MainWindow
        /// </summary>
        private void CloseWindow()
        {
            // Make the MapService control the map in the main window again
            _mapService.SwitchControlToMainMapAsync();
            
            // Request the UI to close the window
            EventAggregator.Publish(new CloseWindowRequestedEvent(this));
        }
        
        
        /// <summary>
        /// Resets the route calculated state when the start or end location changes (-> forces the user to geocode the location again and recalculate the route)
        /// </summary>
        /// <param name="isStartLocation">Indicates whether the start or end location was changed</param>
        private void ResetRouteCalculatedState(bool isStartLocation)
        {
            // If the user changes the start or end location, we need to reset the route calculated state so that the user has to find the coordinates and recalculate the route before saving
            if (isStartLocation)
            {
                EditableTour.StartCoordinates = null;
            }
            else
            {
                EditableTour.EndCoordinates = null;
            }
            RouteCalculated = false;
            
            // Notify the save command that its execution state may have changed
            _executeSave?.RaiseCanExecuteChanged();
        }
    }
}