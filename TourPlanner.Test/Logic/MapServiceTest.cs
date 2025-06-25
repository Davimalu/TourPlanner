using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TourPlanner.config.Interfaces;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Events;
using TourPlanner.Model.Structs;

namespace TourPlanner.Test.Logic
{
    [TestFixture]
    public class MapServiceTest
    {
        // Mocks for dependencies
        private IWebViewService _mockWebViewService;
        private IEventAggregator _mockEventAggregator;
        private ITourPlannerConfig _mockTourPlannerConfig;
        private ILogger<MapService> _mockLogger;

        // System Under Test (SUT)
        private MapService _sut;

        [SetUp]
        public void SetUp()
        {
            // Create mocks for the dependencies
            _mockWebViewService = Substitute.For<IWebViewService>();
            _mockEventAggregator = Substitute.For<IEventAggregator>();
            _mockTourPlannerConfig = Substitute.For<ITourPlannerConfig>();
            _mockLogger = Substitute.For<ILogger<MapService>>();

            // Default setup for successful scenarios
            _mockWebViewService.IsReady.Returns(true);
            _mockTourPlannerConfig.TmpFolder.Returns("C:\\temp\\");

            // Create the SUT with the mocks
            _sut = new MapService(_mockWebViewService, _mockTourPlannerConfig, _mockEventAggregator, _mockLogger);
        }
        
        [Test]
        public void Constructor_WhenWebViewServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MapService(null!, _mockTourPlannerConfig, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenTourPlannerConfigIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MapService(_mockWebViewService, null!, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenEventAggregatorIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MapService(_mockWebViewService, _mockTourPlannerConfig, null!, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MapService(_mockWebViewService, _mockTourPlannerConfig, _mockEventAggregator, null!));
        }
        
        [Test]
        public async Task AddMarkerAsync_WhenWebViewIsNotReady_LogsWarningAndReturnsFalse()
        {
            // Arrange
            _mockWebViewService.IsReady.Returns(false);

            // Act
            var result = await _sut.AddMarkerAsync(new MapMarker(new GeoCoordinate(), "TestName", "TestDesc"));

            // Assert
            Assert.IsFalse(result);
            _mockLogger.Received(1).Warn(Arg.Is<string>(s => s.Contains("WebView is not ready")));
            await _mockWebViewService.DidNotReceive().CallFunctionAsync(Arg.Any<string>(), Arg.Any<object[]>());
        }
        
        [Test]
        public async Task RemoveMarkerByTitleAsync_WhenWebViewIsNotReady_LogsWarningAndReturnsFalse()
        {
            // Arrange
            _mockWebViewService.IsReady.Returns(false);

            // Act
            var result = await _sut.RemoveMarkerByTitleAsync("TestTitle");

            // Assert
            Assert.IsFalse(result);
            _mockLogger.Received(1).Warn(Arg.Is<string>(s => s.Contains("WebView is not ready")));
            await _mockWebViewService.DidNotReceive().CallFunctionAsync(Arg.Any<string>(), Arg.Any<object[]>());
        }
        
        [Test]
        public async Task SetViewToCoordinatesAsync_WhenWebViewIsNotReady_LogsWarningAndReturnsFalse()
        {
            // Arrange
            _mockWebViewService.IsReady.Returns(false);

            // Act
            var result = await _sut.SetViewToCoordinatesAsync(new GeoCoordinate(), 12);

            // Assert
            Assert.IsFalse(result);
            _mockLogger.Received(1).Warn(Arg.Is<string>(s => s.Contains("WebView is not ready")));
            await _mockWebViewService.DidNotReceive().CallFunctionAsync(Arg.Any<string>(), Arg.Any<object[]>());
        }
        
        [Test]
        public async Task DrawRouteAsync_WhenWebViewIsNotReady_LogsWarningAndReturnsFalse()
        {
            // Arrange
            _mockWebViewService.IsReady.Returns(false);

            // Act
            var result = await _sut.DrawRouteAsync("TestGeoJson");

            // Assert
            Assert.IsFalse(result);
            _mockLogger.Received(1).Warn(Arg.Is<string>(s => s.Contains("WebView is not ready")));
            await _mockWebViewService.DidNotReceive().CallFunctionAsync(Arg.Any<string>(), Arg.Any<object[]>());
        }
        
        [Test]
        public async Task ClearMapAsync_WhenWebViewIsNotReady_LogsWarningAndReturnsFalse()
        {
            // Arrange
            _mockWebViewService.IsReady.Returns(false);

            // Act
            var result = await _sut.ClearMapAsync();

            // Assert
            Assert.IsFalse(result);
            _mockLogger.Received(1).Warn(Arg.Is<string>(s => s.Contains("WebView is not ready")));
            await _mockWebViewService.DidNotReceive().CallFunctionAsync(Arg.Any<string>());
        }

        [Test]
        public async Task AddMarkerAsync_WhenSuccessful_CallsWebViewAndReturnsTrue()
        {
            // Arrange
            var marker = new MapMarker(new GeoCoordinate(10, 20), "TestName", "TestDesc");

            // Act
            var result = await _sut.AddMarkerAsync(marker);

            // Assert
            Assert.IsTrue(result);
            await _mockWebViewService.Received(1).CallFunctionAsync("addMarker", 10.0, 20.0, "TestName", "TestDesc");
            _mockLogger.Received(1).Debug(Arg.Any<string>());
        }

        [Test]
        public async Task AddMarkerAsync_WhenWebViewThrowsException_ReturnsFalseAndLogsError()
        {
            // Arrange
            _mockWebViewService.CallFunctionAsync(Arg.Any<string>(), Arg.Any<object[]>())
                .ThrowsAsync(new Exception("JS Error"));

            // Act
            var result = await _sut.AddMarkerAsync(new MapMarker(new GeoCoordinate(10, 20), "TestName", "TestDesc"));

            // Assert
            Assert.IsFalse(result);
            _mockLogger.Received(1).Error(Arg.Is<string>(s => s.Contains("Failed to add marker")), Arg.Any<Exception>());
        }

        [Test]
        public async Task RemoveMarkerByTitleAsync_WhenSuccessful_CallsWebViewAndReturnsTrue()
        {
            // Act
            var result = await _sut.RemoveMarkerByTitleAsync("TestTitle");

            // Assert
            Assert.IsTrue(result);
            await _mockWebViewService.Received(1).CallFunctionAsync("removeMarkerByTitle", "TestTitle");
        }
        
        [Test]
        public async Task SetViewToCoordinatesAsync_WhenSuccessful_CallsWebViewAndReturnsTrue()
        {
            // Arrange
            var coords = new GeoCoordinate(48.2, 16.3);

            // Act
            var result = await _sut.SetViewToCoordinatesAsync(coords, 12);

            // Assert
            Assert.IsTrue(result);
            await _mockWebViewService.Received(1).CallFunctionAsync("flyToLocation", 48.2, 16.3, 12);
        }
        
        [Test]
        public async Task DrawRouteAsync_WhenSuccessful_CallsWebViewAndReturnsTrue()
        {
            // Act
            var result = await _sut.DrawRouteAsync("{ geojson }");

            // Assert
            Assert.IsTrue(result);
            await _mockWebViewService.Received(1).CallFunctionAsync("drawRoute", "{ geojson }");
        }

        [Test]
        public async Task ClearMapAsync_WhenSuccessful_CallsWebViewAndReturnsTrue()
        {
            // Act
            var result = await _sut.ClearMapAsync();

            // Assert
            Assert.IsTrue(result);
            await _mockWebViewService.Received(1).CallFunctionAsync("clearMap");
        }

        [Test]
        public async Task SwitchControlToMainMapAsync_WhenSuccessful_ReturnsTrue()
        {
            // Arrange
            _mockWebViewService.RevertToMainWindowWebViewAsync().Returns(Task.FromResult(true));

            // Act
            var result = await _sut.SwitchControlToMainMapAsync();

            // Assert
            Assert.IsTrue(result);
            await _mockWebViewService.Received(1).RevertToMainWindowWebViewAsync();
        }

        [TestCase(true, false, false, "EndCoordinates")] // Missing End
        [TestCase(false, true, false, "StartCoordinates")] // Missing Start
        [TestCase(false, false, true, "GeoJsonString")]   // Missing Route
        public async Task CaptureMapImageAsync_WithInvalidTourData_ReturnsEmptyAndLogsWarning(bool hasStart, bool hasEnd, bool hasRoute, string missingPart)
        {
            // Arrange
            var tour = new Tour
            {
                StartCoordinates = hasStart ? new GeoCoordinate(1,1) : (GeoCoordinate?)null,
                EndCoordinates = hasEnd ? new GeoCoordinate(2,2) : (GeoCoordinate?)null,
                GeoJsonString = hasRoute ? "{ geojson }" : string.Empty
            };
            
            // Act
            var result = await _sut.CaptureMapImageAsync(tour);

            // Assert
            Assert.That(result, Is.Empty);
            _mockLogger.Received(1).Warn(Arg.Is<string>(s => s.Contains("does not have valid start / end coordinates or route data")));
            await _mockWebViewService.DidNotReceive().TakeScreenshotAsync(Arg.Any<string>());
        }

        [Test]
        public async Task CaptureMapImageAsync_WhenSuccessful_PerformsAllStepsAndReturnsImagePath()
        {
            // Arrange
            var tour = new Tour
            {
                TourName = "Test Tour",
                StartCoordinates = new GeoCoordinate(10, 20),
                EndCoordinates = new GeoCoordinate(30, 40),
                GeoJsonString = "{ geojson }",
                StartLocation = "Start Place",
                EndLocation = "End Place"
            };
            _mockWebViewService.TakeScreenshotAsync(Arg.Any<string>()).Returns(Task.FromResult(true));

            // Act
            var result = await _sut.CaptureMapImageAsync(tour);

            // Assert
            _mockEventAggregator.Received(1).Publish(Arg.Any<MapScreenshotRequestedEvent>());
            
            // Verify map is prepared before screenshot
            await _mockWebViewService.Received(1).CallFunctionAsync("clearMap");
            await _mockWebViewService.Received(1).CallFunctionAsync("addMarker", 10.0, 20.0, "Start", "Start Place");
            await _mockWebViewService.Received(1).CallFunctionAsync("addMarker", 30.0, 40.0, "End", "End Place");
            await _mockWebViewService.Received(1).CallFunctionAsync("drawRoute", "{ geojson }");

            // Verify screenshot is taken
            var expectedPath = Path.Combine("C:\\temp\\", "MapImage.png");
            await _mockWebViewService.Received(1).TakeScreenshotAsync(expectedPath);
            
            // Verify final result
            Assert.That(result, Is.EqualTo(expectedPath));
            _mockLogger.Received(1).Debug(Arg.Is<string>(s => s.Contains("Captured map image")));
        }

        [Test]
        public async Task CaptureMapImageAsync_WhenScreenshotOperationFails_ReturnsEmptyAndLogsError()
        {
            // Arrange
            var tour = new Tour { StartCoordinates = new GeoCoordinate(), EndCoordinates = new GeoCoordinate(), GeoJsonString = "{}" };
            _mockWebViewService.TakeScreenshotAsync(Arg.Any<string>()).Returns(Task.FromResult(false));

            // Act
            var result = await _sut.CaptureMapImageAsync(tour);

            // Assert
            Assert.That(result, Is.Empty);
            _mockLogger.Received(1).Error(Arg.Is<string>(s => s.Contains("Screenshot operation was not successful")));
        }
        
        [Test]
        public async Task CaptureMapImageAsync_WhenExceptionOccurs_ReturnsEmptyAndLogsError()
        {
            // Arrange
            var tour = new Tour { StartCoordinates = new GeoCoordinate(), EndCoordinates = new GeoCoordinate(), GeoJsonString = "{}" };
            _mockWebViewService.TakeScreenshotAsync(Arg.Any<string>()).ThrowsAsync(new IOException("Disk is full"));

            // Act
            var result = await _sut.CaptureMapImageAsync(tour);

            // Assert
            Assert.That(result, Is.Empty);
            _mockLogger.Received(1).Error(Arg.Is<string>(s => s.Contains("Failed to capture map image")), Arg.Any<IOException>());
        }
    }
}
