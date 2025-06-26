using NSubstitute;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Events;
using TourPlanner.ViewModels;

namespace TourPlanner.Test.ViewModels
{
    [TestFixture]
    public class TourDetailsViewModelTest
    {
        // Mock for the dependency
        private IEventAggregator _mockEventAggregator;

        // System Under Test (SUT)
        private TourDetailsViewModel _viewModel;

        // This field will capture the event handler that the ViewModel subscribes
        private Action<SelectedTourChangedEvent> _selectedTourChangedHandler;

        [SetUp]
        public void SetUp()
        {
            _mockEventAggregator = Substitute.For<IEventAggregator>();
            
            // tell the mock that when Subscribe is called, it should run our code, which captures the provided delegate into our local field
            _mockEventAggregator.Subscribe<SelectedTourChangedEvent>(Arg.Do<Action<SelectedTourChangedEvent>>(handler =>
            {
                _selectedTourChangedHandler = handler;
            }));

            // Create the ViewModel, which will trigger the subscription in its constructor
            _viewModel = new TourDetailsViewModel(_mockEventAggregator);
        }
        
        [Test]
        public void Constructor_WhenEventAggregatorIsNull_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TourDetailsViewModel(null!));
        }

        [Test]
        public void Constructor_SubscribesToSelectedTourChangedEvent()
        {
            // Assert: Verify that Subscribe was called exactly once with the correct event type.
            _mockEventAggregator.Received(1).Subscribe<SelectedTourChangedEvent>(Arg.Any<Action<SelectedTourChangedEvent>>());

            // Assert that our handler was successfully captured, proving a delegate was passed.
            Assert.That(_selectedTourChangedHandler, Is.Not.Null);
        }

        [Test]
        public void OnSelectedTourChanged_WhenEventFires_SetsSelectedTourProperty()
        {
            // Arrange
            var sampleTour = new Tour { TourId = 1, TourName = "Alpine Crossing" };
            var tourEvent = new SelectedTourChangedEvent(sampleTour);

            // Act
            // Simulate the EventAggregator firing the event by invoking the captured handler.
            _selectedTourChangedHandler.Invoke(tourEvent);

            // Assert
            // The ViewModel's SelectedTour property should now hold the tour from the event.
            // Using Is.SameAs ensures it's the exact same object instance.
            Assert.That(_viewModel.SelectedTour, Is.SameAs(sampleTour));
        }
    }
}
