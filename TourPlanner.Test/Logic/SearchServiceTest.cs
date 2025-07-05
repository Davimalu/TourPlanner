using System.Collections.ObjectModel;
using System.Globalization;
using NSubstitute;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic;
using TourPlanner.Model;
using TourPlanner.Model.Enums;

namespace TourPlanner.Test.Logic
{
    [TestFixture]
    public class SearchServiceTest
    {
        // Mocks for dependencies
        private ILogger<SearchService> _mockLogger;
        
        // System Under Test (SUT)
        private SearchService _sut;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = Substitute.For<ILogger<SearchService>>();
            _sut = new SearchService(_mockLogger);
        }
        
        [Test]
        public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SearchService(null!));
        }

        [TestCase("null")]
        [TestCase("")]
        [TestCase("   ")]
        public async Task SearchToursAsync_WithNullOrEmptyQuery_ReturnsOriginalList(string query)
        {
            // Arrange
            var originalTours = new List<Tour> { new Tour { TourId = 1 }, new Tour { TourId = 2 } };

            // Act
            var result = await _sut.SearchToursAsync(query == "null" ? null! : query, originalTours);
            
            // Assert
            Assert.That(result, Is.SameAs(originalTours));
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task SearchToursAsync_WithEmptyToursList_ReturnsOriginalEmptyList()
        {
            // Arrange
            var originalTours = new List<Tour>();

            // Act
            var result = await _sut.SearchToursAsync("some query", originalTours);
            
            // Assert
            Assert.That(result, Is.SameAs(originalTours));
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task SearchToursAsync_WhenQueryDoesNotMatch_ReturnsEmptyList()
        {
            // Arrange
            var tours = new List<Tour> { CreateSampleTour(1, "Vienna", "A city tour") };
            
            // Act
            var result = await _sut.SearchToursAsync("nonexistent", tours);
            
            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task SearchToursAsync_WhenQueryMatchesTourName_ReturnsMatchingTour()
        {
            // Arrange
            var tours = new List<Tour>
            {
                CreateSampleTour(1, "Alpine Adventure"),
                CreateSampleTour(2, "City Exploration")
            };
            
            // Act
            var result = await _sut.SearchToursAsync("alpine", tours); // should be Case-insensitive
            
            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().TourId, Is.EqualTo(1));
        }
        
        [Test]
        public async Task SearchToursAsync_WhenQueryMatchesTourDescription_ReturnsMatchingTour()
        {
            // Arrange
            var tours = new List<Tour>
            {
                CreateSampleTour(1, tourDescription: "A beautiful mountain hike."),
                CreateSampleTour(2, tourDescription: "A walk through the old town.")
            };
            
            // Act
            var result = await _sut.SearchToursAsync("mountain", tours);
            
            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().TourId, Is.EqualTo(1));
        }

        [Test]
        public async Task SearchToursAsync_WhenQueryMatchesTransportationType_ReturnsMatchingTour()
        {
            // Arrange
            var tours = new List<Tour>
            {
                CreateSampleTour(1, transportType: Transport.Bicycle),
                CreateSampleTour(2, transportType: Transport.Car)
            };
            
            // Act
            var result = await _sut.SearchToursAsync("bicycle", tours);
            
            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().TourId, Is.EqualTo(1));
        }
        
        [Test]
        public async Task SearchToursAsync_WhenQueryMatchesLogInComment_ReturnsParentTour()
        {
            // Arrange
            var tours = new List<Tour>
            {
                CreateSampleTour(1, "Tour A", logs: [CreateSampleLog(comment: "Sunny weather")]),
                CreateSampleTour(2, "Tour B", logs: [CreateSampleLog(comment: "It was raining")])
            };
            
            // Act
            var result = await _sut.SearchToursAsync("sunny", tours);
            
            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().TourId, Is.EqualTo(1));
        }
        
        [Test]
        public async Task SearchToursAsync_WhenQueryMatchesMultipleTours_ReturnsAllMatches()
        {
            // Arrange
            var tours = new List<Tour>
            {
                CreateSampleTour(1, tourName: "Vienna City Walk"),
                CreateSampleTour(2, tourName: "Alpine Adventure"),
                CreateSampleTour(3, tourDescription: "A walk through historic Vienna.")
            };
            
            // Act
            var result = await _sut.SearchToursAsync("vienna", tours);
            
            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Any(t => t.TourId == 1), Is.True);
            Assert.That(result.Any(t => t.TourId == 3), Is.True);
        }
        
        [Test]
        public async Task SearchToursAsync_WhenQueryMatchesNumber_WithCultureUsingComma_ReturnsMatchingTour()
        {
            // Arrange
            var originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                // Set a culture where the decimal separator is a comma (makes unit tests reproducible on every machine configuration)
                Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
                
                var tours = new List<Tour>
                {
                    CreateSampleTour(1, distance: 12.5f),
                    CreateSampleTour(2, distance: 10.0f)
                };

                // Act
                var result = await _sut.SearchToursAsync("12,5", tours);
                
                // Assert
                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result.First().TourId, Is.EqualTo(1));
            }
            finally
            {
                // Reset the culture to not affect other tests
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

        
        /// <summary>
        /// Creates a sample Tour object with the specified parameters
        /// </summary>
        /// <param name="id">The ID of the tour</param>
        /// <param name="tourName">The name of the tour, optional</param>
        /// <param name="tourDescription">The description of the tour, optional</param>
        /// <param name="transportType">Transport type of the tour, optional</param>
        /// <param name="distance">Distance of the tour in kilometers, optional</param>
        /// <param name="logs">List of TourLogs associated with the tour, optional</param>
        /// <returns></returns>
        private Tour CreateSampleTour(int id, string tourName = "Sample", string tourDescription = "Desc", 
                                      Transport transportType = Transport.Car, float distance = 10f, 
                                      ObservableCollection<TourLog>? logs = null)
        {
            return new Tour
            {
                TourId = id,
                TourName = tourName,
                TourDescription = tourDescription,
                StartLocation = "Start",
                EndLocation = "End",
                TransportationType = transportType,
                Distance = distance,
                EstimatedTime = TimeSpan.FromHours(1),
                Popularity = 50f,
                ChildFriendlyRating = 50f,
                AiSummary = "Summary text.",
                Logs = new ObservableCollection<TourLog>(logs ?? new ObservableCollection<TourLog>())
            };
        }

        
        /// <summary>
        /// Creates a sample TourLog object with the specified parameters
        /// </summary>
        /// <param name="comment">The comment for the log, optional</param>
        /// <param name="difficulty">The difficulty rating of the log, optional</param>
        /// <returns></returns>
        private TourLog CreateSampleLog(string comment = "Log Comment", int difficulty = 3)
        {
            return new TourLog
            {
                TimeStamp = new DateTime(2023, 1, 1),
                Comment = comment,
                Difficulty = difficulty,
                DistanceTraveled = 5f,
                TimeTaken = TimeSpan.FromMinutes(30),
                Rating = 4
            };
        }
    }
}
