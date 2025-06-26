using NSubstitute;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.ViewModels;

namespace TourPlanner.Test.ViewModels
{
    [TestFixture]
    public class MapViewModelTest
    {
        // Mocks for dependencies
        private IMapService _mockMapService;
        private IEventAggregator _mockEventAggregator;
        private ILogger<MapViewModel> _mockLogger;

        // System Under Test (SUT)
        private MapViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            // Create mocks for the dependencies
            _mockMapService = Substitute.For<IMapService>();
            _mockEventAggregator = Substitute.For<IEventAggregator>();
            _mockLogger = Substitute.For<ILogger<MapViewModel>>();

            // Initialize the MapViewModel with the mocked dependencies
            _viewModel = new MapViewModel(_mockMapService, _mockEventAggregator, _mockLogger);
        }
        
        [Test]
        public void Constructor_WhenMapServiceIsNull_ThrowsArgumentNullException()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() => new MapViewModel(null!, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenEventAggregatorIsNull_ThrowsArgumentNullException()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() => new MapViewModel(_mockMapService, null!, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() => new MapViewModel(_mockMapService, _mockEventAggregator, null!));
        }
        
        // TODO: The map's functionality is private, figure out how to test it
    }
}
