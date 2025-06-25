using System.Collections.ObjectModel;
using Newtonsoft.Json;
using NSubstitute;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic;
using TourPlanner.Model;
using TourPlanner.Model.Enums;

namespace TourPlanner.Test.Logic
{
    [TestFixture]
    public class AttributeServiceTest
    {
        // Mocks for dependencies
        private ILogger<AttributeService> _mockLogger;
        private ITourService _mockTourService;
        private IAiService _mockAiService;

        // System Under Test (SUT)
        private AttributeService _sut;

        [SetUp]
        public void SetUp()
        {
            // Create mocks for the dependencies
            _mockLogger = Substitute.For<ILogger<AttributeService>>();
            _mockTourService = Substitute.For<ITourService>();
            _mockAiService = Substitute.For<IAiService>();
            
            // Create a new instance of the service with the mocks
            _sut = new AttributeService(_mockTourService, _mockAiService, _mockLogger);
        }
        
        [Test]
        public void Constructor_WhenTourServiceIsNull_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AttributeService(null!, _mockAiService, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenAiServiceIsNull_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AttributeService(_mockTourService, null!, _mockLogger));
        }

        [Test]
        public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AttributeService(_mockTourService, _mockAiService, null!));
        }

        [Test]
        public async Task CalculatePopularityAsync_WhenToursExist_CalculatesCorrectPopularity()
        {
            // Arrange
            Tour tourToTest = new Tour { TourId = 1, TourName = "Test Tour", Logs = new ObservableCollection<TourLog> { new(), new() } }; // 2 logs
            Tour mostPopularTour = new Tour { TourId = 2, TourName = "Popular Tour", Logs = new ObservableCollection<TourLog> { new(), new(), new(), new() } }; // 4 logs
            Tour otherTour = new Tour { TourId = 3, TourName = "Least popular tour", Logs = new ObservableCollection<TourLog> { new() } }; // 1 log

            List<Tour> allTours = new List<Tour> { tourToTest, mostPopularTour, otherTour };
            _mockTourService.GetToursAsync()!.Returns(Task.FromResult(allTours));

            // Act
            float popularity = await _sut.CalculatePopularityAsync(tourToTest);

            // Assert: Popularity should be (2 logs / 4 logs) * 100 = 50%
            Assert.That(popularity, Is.EqualTo(50.0f).Within(0.001));
        }

        [Test]
        public async Task CalculatePopularityAsync_WhenNoToursExist_ReturnsZeroAndLogsWarning()
        {
            // Arrange
            _mockTourService.GetToursAsync()!.Returns(Task.FromResult<List<Tour>>(null!));
            var tourToTest = new Tour { TourId = 1 };

            // Act
            float popularity = await _sut.CalculatePopularityAsync(tourToTest);
            
            // Assert
            Assert.That(popularity, Is.EqualTo(0.0f));
            _mockLogger.Received(1).Warn(Arg.Is<string>(s => s.Contains("No tours found")));
        }

        [Test]
        public async Task CalculatePopularityAsync_WhenNoToursHaveLogs_ReturnsZeroAndLogsWarning()
        {
            // Arrange
            var tourToTest = new Tour { TourId = 1, TourName = "TourWithNoLogs", Logs = new ObservableCollection<TourLog>() };
            var otherTour = new Tour { TourId = 2, TourName = "AnotherTourWithNoLogs", Logs = new ObservableCollection<TourLog>() };
            var allTours = new List<Tour> { tourToTest, otherTour };
            _mockTourService.GetToursAsync()!.Returns(Task.FromResult(allTours));
            
            // Act
            float popularity = await _sut.CalculatePopularityAsync(tourToTest);

            // Assert
            Assert.That(popularity, Is.EqualTo(0.0f));
            _mockLogger.Received(1).Warn(Arg.Is<string>(s => s.Contains("No logs found for any tours")));
        }

        [Test]
        public void CalculateChildFriendliness_WhenTourHasNoLogs_ReturnsZeroAndLogsWarning()
        {
            // Arrange
            var tour = new Tour { TourId = 1, TourName = "Empty Tour", Logs = new ObservableCollection<TourLog>() };
            
            // Act
            float friendliness = _sut.CalculateChildFriendliness(tour);

            // Assert
            Assert.That(friendliness, Is.EqualTo(0.0f));
            _mockLogger.Received(1).Warn(Arg.Is<string>(s => s.Contains("No logs available")));
        }
        
        [Test]
        public void CalculateChildFriendliness_WithPerfectlyIdealLog_Returns100()
        {
            // Arrange
            var tour = new Tour
            {
                Logs = new ObservableCollection<TourLog>
                {
                    // Difficulty 0 -> Score 1.0
                    // Distance 0 -> Score 1.0
                    // Duration 0 -> Score 1.0
                    new() { Difficulty = 0, DistanceTraveled = 0, TimeTaken = TimeSpan.Zero }
                }
            };
            
            // Act
            float friendliness = _sut.CalculateChildFriendliness(tour);

            // Assert
            // (1.0 * 0.5) + (1.0 * 0.2) + (1.0 * 0.3) = 1.0. Result is 1.0 * 100 = 100.
            Assert.That(friendliness, Is.EqualTo(100.0f).Within(0.001));
        }

        [Test]
        public void CalculateChildFriendliness_WithBoundaryValues_Returns0()
        {
            // Arrange
            var tour = new Tour
            {
                Logs = new ObservableCollection<TourLog>
                {
                    // Difficulty 5 -> Score 0.0
                    // Distance 5.0 km -> Score 0.0
                    // Duration 120.0 min -> Score 0.0
                    new() { Difficulty = 5, DistanceTraveled = 5.0f, TimeTaken = TimeSpan.FromMinutes(120) }
                }
            };
            
            // Act
            float friendliness = _sut.CalculateChildFriendliness(tour);
            
            // Assert
            // (0.0 * 0.5) + (0.0 * 0.2) + (0.0 * 0.3) = 0.0. Result is 0.0 * 100 = 0.
            Assert.That(friendliness, Is.EqualTo(0.0f).Within(0.001));
        }

        [Test]
        public void CalculateChildFriendliness_WithHalfwayValues_Returns50()
        {
            // Arrange
            var tour = new Tour
            {
                Logs = new ObservableCollection<TourLog>
                {
                    // Difficulty 2 -> Score 0.6
                    // Distance 2.5km -> Score 0.5
                    // Duration 60min -> Score 0.5
                    new() { Difficulty = 2, DistanceTraveled = 2.5f, TimeTaken = TimeSpan.FromMinutes(60) }
                }
            };

            // Act
            float friendliness = _sut.CalculateChildFriendliness(tour);
            
            // Assert
            // (0.6 * 0.5) + (0.5 * 0.2) + (0.5 * 0.3) = 0.3 + 0.1 + 0.15 = 0.55. Result is 0.5 * 100 = 55
            Assert.That(friendliness, Is.EqualTo(55.0f).Within(0.001));
        }
        
        [Test]
        public void CalculateChildFriendliness_WithTwoLogs_CalculatesCorrectAverage()
        {
            // Arrange
            var perfectLog = new TourLog { Difficulty = 0, DistanceTraveled = 0, TimeTaken = TimeSpan.Zero }; // Score = 1.0
            var worstLog = new TourLog { Difficulty = 5, DistanceTraveled = 5.0f, TimeTaken = TimeSpan.FromMinutes(120) }; // Score = 0.0
            var tour = new Tour { Logs = new ObservableCollection<TourLog> { perfectLog, worstLog } };
            
            // Act
            float friendliness = _sut.CalculateChildFriendliness(tour);

            // Assert
            // Total score = 1.0 + 0.0 = 1.0. Average score = 1.0 / 2 = 0.5. Result is 50.
            Assert.That(friendliness, Is.EqualTo(50.0f).Within(0.001));
        }
        
        [Test]
        public void CalculateChildFriendliness_WithValuesExceedingMax_ClampsToZeroScore()
        {
            // Arrange
            var tour = new Tour
            {
                Logs = new ObservableCollection<TourLog>
                {
                    // Values are worse than the "ideal max", so scores should all be 0.
                    new() { Difficulty = 6, DistanceTraveled = 10.0f, TimeTaken = TimeSpan.FromMinutes(200) }
                }
            };
            
            // Act
            float friendliness = _sut.CalculateChildFriendliness(tour);
            
            // Assert
            Assert.That(friendliness, Is.EqualTo(0.0f).Within(0.001));
        }

        [Test]
        public async Task GetAiSummaryAsync_WithValidTour_CallsAiServiceAndReturnsResult()
        {
            // Arrange
            var tour = new Tour { TourId = 1, TourName = "AI Test Tour" };
            var expectedResponse = "This is a great tour!";
            var expectedJson = JsonConvert.SerializeObject(tour);
            
            // The prompt is a const in the SUT
            const string expectedSystemPrompt = "You are an expert tour guide and analyst. Your task is to provide a concise summary of the tour details and its associated logs based on the provided details. Talk about the tour's highlights, challenges, and any notable features. Please provide your answer in plain-text, don't use Markdown formatting - you can use ASCII art to make the answer pretty if you want. Your answer should be in English, even if the Tour Information is provided in another language. Use the following JSON data to generate your summary:\n\n";

            _mockAiService.AnswerQueryAsync(expectedSystemPrompt, expectedJson, Arg.Any<AiModel>())
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var summary = await _sut.GetAiSummaryAsync(tour);

            // Assert
            Assert.That(summary, Is.EqualTo(expectedResponse));
            await _mockAiService.Received(1).AnswerQueryAsync(expectedSystemPrompt, expectedJson, AiModel.GPT4_1);
        }
    }
}