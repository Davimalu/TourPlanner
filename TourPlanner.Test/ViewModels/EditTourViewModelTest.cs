using NSubstitute;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Enums;
using TourPlanner.Model.Events;
using TourPlanner.Model.Structs;
using TourPlanner.ViewModels;
using MessageBoxButton = TourPlanner.Model.Enums.MessageBoxAbstraction.MessageBoxButton;
using MessageBoxImage = TourPlanner.Model.Enums.MessageBoxAbstraction.MessageBoxImage;


namespace TourPlanner.Test.ViewModels
{
    [TestFixture]
    public class EditTourViewModelTest
    {
        // Mocks for Dependencies
        private ITourService _mockTourService;
        private IOrsService _mockOrsService;
        private IMapService _mockMapService;
        private IWpfService _mockWpfService;
        private IEventAggregator _mockEventAggregator;
        private ILogger<EditTourViewModel> _mockLogger;

        // Sample Data
        private Tour _sampleTour;
        
        // System Under Test (SUT)
        private EditTourViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            // Initialize mocks for all dependencies
            _mockTourService = Substitute.For<ITourService>();
            _mockOrsService = Substitute.For<IOrsService>();
            _mockMapService = Substitute.For<IMapService>();
            _mockWpfService = Substitute.For<IWpfService>();
            _mockEventAggregator = Substitute.For<IEventAggregator>();
            _mockLogger = Substitute.For<ILogger<EditTourViewModel>>();

            // Create a sample Tour object to use in tests
            _sampleTour = new Tour { TourId = 1, TourName = "Sample Tour" };
        }

        
        /// <summary>
        /// Helper method to create a new instance of EditTourViewModel with the mocked dependencies
        /// </summary>
        /// <param name="tour">Tour to edit</param>
        /// <returns>A new instance of <see cref="EditTourViewModel"/></returns>
        private EditTourViewModel CreateViewModel(Tour tour)
        {
            return new EditTourViewModel(tour, _mockTourService, _mockOrsService, _mockMapService, _mockWpfService, _mockEventAggregator, _mockLogger);
        }
        
        [Test]
        public void Constructor_WhenTourServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EditTourViewModel(_sampleTour, null!, _mockOrsService, _mockMapService, _mockWpfService, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenOrsServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EditTourViewModel(_sampleTour, _mockTourService, null!, _mockMapService, _mockWpfService, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenMapServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EditTourViewModel(_sampleTour, _mockTourService, _mockOrsService, null!, _mockWpfService, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenWpfServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EditTourViewModel(_sampleTour, _mockTourService, _mockOrsService, _mockMapService, null!, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenEventAggregatorIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EditTourViewModel(_sampleTour, _mockTourService, _mockOrsService, _mockMapService, _mockWpfService, null!, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EditTourViewModel(_sampleTour, _mockTourService, _mockOrsService, _mockMapService, _mockWpfService, _mockEventAggregator, null!));
        }

        [Test]
        public void Constructor_CreatesCopyOfTour()
        {
            // Act
            _viewModel = CreateViewModel(_sampleTour);

            // Assert
            Assert.That(_viewModel.EditableTour, Is.Not.SameAs(_sampleTour));
            Assert.That(_viewModel.EditableTour.TourId, Is.EqualTo(_sampleTour.TourId));
        }

        [Test]
        public void StartLocation_Setter_ResetsCoordinatesAndRouteState()
        {
            // Arrange
            _viewModel = CreateViewModel(_sampleTour);
            _viewModel.EditableTour.StartCoordinates = new GeoCoordinate(1, 1);
            _viewModel.RouteCalculated = true;

            // Act
            _viewModel.StartLocation = "New Start Location";

            // Assert
            Assert.That(_viewModel.EditableTour.StartCoordinates, Is.Null);
            Assert.That(_viewModel.RouteCalculated, Is.False);
        }

        [Test]
        public void EndLocation_Setter_ResetsCoordinatesAndRouteState()
        {
            // Act
            _viewModel = CreateViewModel(_sampleTour);
            _viewModel.EditableTour.EndCoordinates = new GeoCoordinate(1, 1);
            _viewModel.RouteCalculated = true;

            // Act
            _viewModel.EndLocation = "New End Location";

            // Assert
            Assert.That(_viewModel.EditableTour.EndCoordinates, Is.Null);
            Assert.That(_viewModel.RouteCalculated, Is.False);
        }
        
        [Test]
        public void ExecuteCalculateAndDrawRoute_CanExecute_IsTrue_WhenBothCoordinatesExist()
        {
            // Arrange
            _viewModel = CreateViewModel(_sampleTour);
            _viewModel.EditableTour.StartCoordinates = new GeoCoordinate(1, 1);
            _viewModel.EditableTour.EndCoordinates = new GeoCoordinate(2, 2);

            // Assert
            Assert.IsTrue(_viewModel.ExecuteCalculateAndDrawRoute.CanExecute(null));
        }
        
        [Test]
        public void ExecuteCalculateAndDrawRoute_CanExecute_IsFalse_WhenStartCoordinatesMissing()
        {
            // Arrange
            _viewModel = CreateViewModel(_sampleTour);
            _viewModel.EditableTour.StartCoordinates = null;
            _viewModel.EditableTour.EndCoordinates = new GeoCoordinate(2, 2);

            // Assert
            Assert.IsFalse(_viewModel.ExecuteCalculateAndDrawRoute.CanExecute(null));
        }
        
        [Test]
        public void ExecuteSave_CanExecute_IsFalse_WhenConditionsNotMet()
        {
            // Arrange
            _viewModel = CreateViewModel(_sampleTour);
            _viewModel.EditableTour.StartCoordinates = new GeoCoordinate(1,1);
            _viewModel.EditableTour.EndCoordinates = new GeoCoordinate(2,2);
            _viewModel.TourName = "Test";
            // ...but route is not calculated
            
            // Act && Assert
            Assert.IsFalse(_viewModel.ExecuteSave.CanExecute(null));
        }

        [Test]
        public void ExecuteSave_CanExecute_IsTrue_WhenAllConditionsMet()
        {
            // Arrange
            _viewModel = CreateViewModel(_sampleTour);
            _viewModel.EditableTour.StartCoordinates = new GeoCoordinate(1, 1);
            _viewModel.EditableTour.EndCoordinates = new GeoCoordinate(2, 2);
            _viewModel.TourName = "Test Tour";
            _viewModel.RouteCalculated = true;

            // Assert
            Assert.IsTrue(_viewModel.ExecuteSave.CanExecute(null));
        }
        
        [Test]
        public async Task GeocodeLocationAsync_WhenSuccessful_UpdatesCoordinatesAndMap()
        {
            // Arrange
            _viewModel = CreateViewModel(_sampleTour);
            var geoCode = new GeoCode("Vienna, Austria", new GeoCoordinate(48.2, 16.3));
            _mockOrsService.GetGeoCodeFromAddressAsync("Vienna").Returns(Task.FromResult<GeoCode?>(geoCode));
            _viewModel.StartLocation = "Vienna";

            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteGeocodeStart).ExecuteAsync(null);

            // Assert
            Assert.That(_viewModel.EditableTour.StartCoordinates, Is.EqualTo(geoCode.Coordinates));
            Assert.That(_viewModel.StartLocation, Is.EqualTo(geoCode.Label));
            
            await _mockMapService.Received(1).RemoveMarkerByTitleAsync("Start");
            await _mockMapService.Received(1).AddMarkerAsync(Arg.Is<MapMarker>(m => m.Name == "Start"));
            await _mockMapService.Received(1).SetViewToCoordinatesAsync(geoCode.Coordinates);
        }

        [Test]
        public async Task GeocodeLocationAsync_WhenOrsServiceFails_ShowsMessageBox()
        {
            // Arrange
            _viewModel = CreateViewModel(_sampleTour);
            _mockOrsService.GetGeoCodeFromAddressAsync(Arg.Any<string>()).Returns(Task.FromResult<GeoCode?>(null));
            _viewModel.StartLocation = "InvalidLocation";
            
            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteGeocodeStart).ExecuteAsync(null);
            
            // Assert
            _mockWpfService.Received(1).ShowMessageBox("Geocoding Error", Arg.Any<string>(), MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        [Test]
        public async Task CalculateAndDrawRouteAsync_WhenSuccessful_UpdatesTourAndMap()
        {
            // Arrange
            _viewModel = CreateViewModel(_sampleTour);
            _viewModel.EditableTour.StartCoordinates = new GeoCoordinate(1,1);
            _viewModel.EditableTour.EndCoordinates = new GeoCoordinate(2,2);
            var route = new Route { Distance = 5000, Duration = 3600, RouteGeometry = "{geojson}" };
            _mockOrsService.GetRouteAsync(Arg.Any<Transport>(), Arg.Any<GeoCoordinate>(), Arg.Any<GeoCoordinate>()).Returns(Task.FromResult<Route?>(route));

            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteCalculateAndDrawRoute).ExecuteAsync(null);

            // Assert
            Assert.That(_viewModel.EditableTour.Distance, Is.EqualTo(5.0)); // 5000m -> 5km
            Assert.That(_viewModel.EditableTour.EstimatedTime, Is.EqualTo(TimeSpan.FromHours(1)));
            Assert.That(_viewModel.EditableTour.GeoJsonString, Is.EqualTo(route.RouteGeometry));
            Assert.That(_viewModel.RouteCalculated, Is.True);
            
            await _mockMapService.Received(1).ClearMapAsync();
            await _mockMapService.Received(2).AddMarkerAsync(Arg.Any<MapMarker>()); // start + end
            await _mockMapService.Received(1).DrawRouteAsync(route.RouteGeometry);
        }

        [Test]
        public async Task SaveAsync_WhenCreatingNewTour_CallsCreateTourService()
        {
            // Arrange
            var newTourTemplate = new Tour { TourId = -1, TourName = "New Tour" };
            _viewModel = CreateViewModel(newTourTemplate);
            var createdTour = new Tour { TourId = 99 };
            _mockTourService.CreateTourAsync(_viewModel.EditableTour).Returns(Task.FromResult<Tour?>(createdTour));
            
            // Set up the tour with valid start and end coordinates
            _viewModel.EditableTour.StartCoordinates = new GeoCoordinate(1, 1);
            _viewModel.EditableTour.EndCoordinates = new GeoCoordinate(2, 2);
            _viewModel.RouteCalculated = true; // Ensure route is calculated
            
            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteSave).ExecuteAsync(null);
            
            // Assert
            await _mockTourService.Received(1).CreateTourAsync(_viewModel.EditableTour);
            await _mockTourService.DidNotReceive().UpdateTourAsync(Arg.Any<Tour>());
            
            // Assert close window logic
            await _mockMapService.Received(1).SwitchControlToMainMapAsync();
            _mockEventAggregator.Received(1).Publish(Arg.Any<CloseWindowRequestedEvent>());
        }

        [Test]
        public async Task SaveAsync_WhenUpdatingExistingTour_CallsUpdateTourService()
        {
            // Arrange
            _viewModel = CreateViewModel(_sampleTour); // _sampleTour has TourId = 1
            
            // Set up the tour with valid start and end coordinates
            _viewModel.EditableTour.StartCoordinates = new GeoCoordinate(1, 1);
            _viewModel.EditableTour.EndCoordinates = new GeoCoordinate(2, 2);
            _viewModel.RouteCalculated = true; // Ensure route is calculated
            
            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteSave).ExecuteAsync(null);
            
            // Assert
            await _mockTourService.Received(1).UpdateTourAsync(_viewModel.EditableTour);
            await _mockTourService.DidNotReceive().CreateTourAsync(Arg.Any<Tour>());
            
            // Assert close window logic
            await _mockMapService.Received(1).SwitchControlToMainMapAsync();
            _mockEventAggregator.Received(1).Publish(Arg.Any<CloseWindowRequestedEvent>());
        }

        [Test]
        public void CancelEditTour_WhenExecuted_ClosesWindow()
        {
            // Arrange
            _viewModel = CreateViewModel(_sampleTour);

            // Act
            _viewModel.ExecuteCancel.Execute(null);

            // Assert
            _mockEventAggregator.Received(1).Publish(Arg.Any<CloseWindowRequestedEvent>());
            _mockMapService.Received(1).SwitchControlToMainMapAsync();
        }
    }
}