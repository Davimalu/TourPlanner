using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Events;
using TourPlanner.ViewModels;

namespace TourPlanner.Test.ViewModels
{
    [TestFixture]
    public class TourAttributesViewModelTest
    {
        // Mocks for dependencies
        private ITourService _mockTourService;
        private IAttributeService _mockAttributeService;
        private IEventAggregator _mockEventAggregator;
        private ILogger<TourAttributesViewModel> _mockLogger;

        // System Under Test (SUT)
        private TourAttributesViewModel _viewModel;
        
        // To capture the event handler subscribed by the ViewModel
        private Action<SelectedTourChangedEvent> _selectedTourChangedHandler;

        [SetUp]
        public void SetUp()
        {
            // Create mocks for the dependencies
            _mockTourService = Substitute.For<ITourService>();
            _mockAttributeService = Substitute.For<IAttributeService>();
            _mockEventAggregator = Substitute.For<IEventAggregator>();
            _mockLogger = Substitute.For<ILogger<TourAttributesViewModel>>();

            // Capture the subscribed event handler when the ViewModel is created
            _mockEventAggregator.Subscribe(Arg.Do<Action<SelectedTourChangedEvent>>(handler => _selectedTourChangedHandler = handler));
            
            // Initialize the ViewModel with the mocked dependencies
            _viewModel = new TourAttributesViewModel(_mockTourService, _mockAttributeService, _mockEventAggregator, _mockLogger);
        }
        
        [Test]
        public void Constructor_WhenTourServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TourAttributesViewModel(null, _mockAttributeService, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenAttributeServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TourAttributesViewModel(_mockTourService, null, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenEventAggregatorIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TourAttributesViewModel(_mockTourService, _mockAttributeService, null, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TourAttributesViewModel(_mockTourService, _mockAttributeService, _mockEventAggregator, null));
        }

        [Test]
        public void Constructor_SubscribesToSelectedTourChangedEvent()
        {
            // Assert: The constructor called in SetUp should have triggered the subscription
            _mockEventAggregator.Received(1).Subscribe(Arg.Any<Action<SelectedTourChangedEvent>>());
            Assert.That(_selectedTourChangedHandler, Is.Not.Null);
        }

        [Test]
        public void OnSelectedTourChanged_WhenEventFiresWithTour_UpdatesSelectedTourProperty()
        {
            // Arrange
            var newTour = new Tour { TourId = 1, TourName = "New Selected Tour" };
            var tourEvent = new SelectedTourChangedEvent(newTour);
            
            // Act
            // Simulate the EventAggregator firing the event by invoking the captured handler
            _selectedTourChangedHandler.Invoke(tourEvent);
            
            // Assert
            Assert.That(_viewModel.SelectedTour, Is.SameAs(newTour));
        }
        
        [Test]
        public void ExecuteCalculateAttributes_CanExecute_IsFalse_WhenNoTourIsSelected()
        {
            // Arrange
            _viewModel.SelectedTour = null;
            
            // Act & Assert
            Assert.IsFalse(_viewModel.ExecuteCalculateAttributes.CanExecute(null));
        }
        
        [Test]
        public void ExecuteCalculateAttributes_CanExecute_IsTrue_WhenTourIsSelected()
        {
            // Arrange
            var tour = new Tour { TourId = 1, TourName = "Test Tour" };
            _viewModel.SelectedTour = tour;
            
            // Act & Assert
            Assert.IsTrue(_viewModel.ExecuteCalculateAttributes.CanExecute(null));
        }

        [Test]
        public async Task CalculateAttributes_WhenNoTourIsSelected_ReturnsEarly()
        {
            // Arrange
            _viewModel.SelectedTour = null;

            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteCalculateAttributes).ExecuteAsync(null);
            
            // Assert
            await _mockAttributeService.DidNotReceive().CalculatePopularityAsync(Arg.Any<Tour>());
            await _mockTourService.DidNotReceive().UpdateTourAsync(Arg.Any<Tour>());
        }

        [Test]
        public async Task CalculateAttributes_WhenSuccessful_CallsServicesAndUpdatesTour()
        {
            // Arrange
            var tour = new Tour { TourId = 1, TourName = "Test Tour" };
            _viewModel.SelectedTour = tour;

            // Mock service calls
            _mockAttributeService.CalculatePopularityAsync(tour).Returns(Task.FromResult(85.5f));
            _mockAttributeService.CalculateChildFriendliness(tour).Returns(70.0f);
            _mockAttributeService.GetAiSummaryAsync(tour).Returns(Task.FromResult("This is a great tour."));

            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteCalculateAttributes).ExecuteAsync(null);
            
            // Assert - attribute calculation
            await _mockAttributeService.Received(1).CalculatePopularityAsync(tour);
            _mockAttributeService.Received(1).CalculateChildFriendliness(tour);
            await _mockAttributeService.Received(1).GetAiSummaryAsync(tour);
            
            // Assert - tour properties were updated
            Assert.That(_viewModel.SelectedTour.Popularity, Is.EqualTo(85.5f));
            Assert.That(_viewModel.SelectedTour.ChildFriendlyRating, Is.EqualTo(70.0f));
            Assert.That(_viewModel.SelectedTour.AiSummary, Is.EqualTo("This is a great tour."));

            // Assert - database was updated
            await _mockTourService.Received(1).UpdateTourAsync(tour);
        }
    }
}
