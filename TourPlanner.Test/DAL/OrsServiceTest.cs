using NSubstitute;
using TourPlanner.config.Interfaces;
using TourPlanner.DAL.ServiceAgents;
using TourPlanner.Infrastructure.Interfaces;

namespace TourPlanner.Test.DAL
{
    [TestFixture]
    public class OrsServiceTests
    {
        // Mocks for dependencies
        private ILogger<OrsService> _mockLogger;
        private ITourPlannerConfig _mockConfig;
        private HttpMessageHandler _mockHttpMessageHandler;

        // HttpClient is a bit special, we use the real class but with a mock handler
        private HttpClient _httpClient;
        
        // System Under Test (SUT)
        private OrsService _sut;

        [SetUp]
        public void SetUp()
        {
            // Create mocks for the dependencies
            _mockLogger = Substitute.For<ILogger<OrsService>>();
            _mockConfig = Substitute.For<ITourPlannerConfig>();
            _mockHttpMessageHandler = Substitute.For<HttpMessageHandler>();
            
            // Configure Mock Behavior
            _mockConfig.OpenRouteServiceApiKey.Returns("fake-api-key");
            _mockConfig.OpenRouteServiceBaseUrl.Returns("https://fake.api.com");
            
            _httpClient = new HttpClient(_mockHttpMessageHandler);
            
            // Create the SUT with mocks
            _sut = new OrsService(_httpClient, _mockConfig, _mockLogger);
        }
        
        [TearDown]
        public void TearDown()
        {
            // Dispose the resources to avoid memory leaks
            _httpClient.Dispose();
            _mockHttpMessageHandler.Dispose();
        }

        [Test]
        public void Constructor_WhenHttpClientIsNull_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new OrsService(null!, _mockConfig, _mockLogger));
        }

        [Test]
        public void Constructor_WhenConfigApiKeyIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            _mockConfig.OpenRouteServiceApiKey.Returns((string)null!);

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new OrsService(_httpClient, _mockConfig, _mockLogger));
            Assert.That(ex.Message, Does.Contain("OpenRouteService API key is not configured."));
        }

        [Test]
        public void Constructor_WhenConfigBaseUrlIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            _mockConfig.OpenRouteServiceBaseUrl.Returns((string)null!);

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new OrsService(_httpClient, _mockConfig, _mockLogger));
            Assert.That(ex.Message, Does.Contain("OpenRouteService base URL is not configured."));
        }
    }
}