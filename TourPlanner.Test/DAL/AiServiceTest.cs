using NSubstitute;
using TourPlanner.config.Interfaces;
using TourPlanner.DAL.ServiceAgents;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Model.Enums;

namespace TourPlanner.Test.DAL
{
    [TestFixture]
    public class AiServiceTest
    {
        // Mocks for dependencies
        private ITourPlannerConfig _mockConfig;
        private ILogger<AiService> _mockLogger;
        private HttpMessageHandler _mockHttpMessageHandler;
        
        // HttpClient is a bit special, we use the real class but with a mock handler
        private HttpClient _httpClient;

        // System Under Test (SUT)
        private AiService _sut;

        [SetUp]
        public void SetUp()
        {
            // Arrange Mocks
            _mockConfig = Substitute.For<ITourPlannerConfig>();
            _mockLogger = Substitute.For<ILogger<AiService>>();
            _mockHttpMessageHandler = Substitute.For<HttpMessageHandler>();
            
            // Configure Mock Behavior
            _mockConfig.OpenRouterApiKey.Returns("fake-api-key");
            _mockConfig.OpenRouterBaseUrl.Returns("https://api.openrouter.ai/v1");

            // Set up HttpClient with the Mock Handler
            _httpClient = new HttpClient(_mockHttpMessageHandler);
            
            // Instantiate the SUT with the mocks
            _sut = new AiService(_mockConfig, _httpClient, _mockLogger);
        }

        [TearDown]
        public void TearDown()
        {
            // Dispose the resources to avoid memory leaks
            _sut.Dispose();
            _httpClient.Dispose();
            _mockHttpMessageHandler.Dispose();
        }
        
        [Test]
        public void Constructor_WhenConfigIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            ITourPlannerConfig? nullConfig = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new AiService(nullConfig!, _httpClient, _mockLogger));
            Assert.That(ex.ParamName, Is.EqualTo("config"));
        }

        [Test]
        public void Constructor_WhenHttpClientIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            HttpClient? nullClient = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new AiService(_mockConfig, nullClient!, _mockLogger));
            Assert.That(ex.ParamName, Is.EqualTo("httpClient"));
        }

        [Test]
        public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            ILogger<AiService>? nullLogger = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new AiService(_mockConfig, _httpClient, nullLogger!));
            Assert.That(ex.ParamName, Is.EqualTo("logger"));
        }

        [Test]
        public void Constructor_OnSuccess_SetsAuthorizationHeader()
        {
            // Assert
            Assert.That(_httpClient.DefaultRequestHeaders.Authorization, Is.Not.Null);
            Assert.That(_httpClient.DefaultRequestHeaders.Authorization.Scheme, Is.EqualTo("Bearer"));
            Assert.That(_httpClient.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo("fake-api-key"));
        }
        
        [Test]
        public void AnswerQueryAsync_WithUnsupportedModel_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var unsupportedModel = (AiModel)999; // invalid enum value

            // Act & Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => 
                _sut.AnswerQueryAsync("prompt", "query", unsupportedModel));
        }
    }
}