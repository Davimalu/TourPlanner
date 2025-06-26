using System.Collections.ObjectModel;
using NSubstitute;
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
    public class TourLogsViewModelTest
    {
        // Mocks for dependencies
        private IWpfService _mockWpfService;
        private ITourLogService _mockTourLogService;
        private IEventAggregator _mockEventAggregator;
        private ILogger<TourLogsViewModel> _mockLogger;

        // System Under Test (SUT)
        private TourLogsViewModel _viewModel;

        // To capture the subscribed event handler
        private Action<SelectedTourChangedEvent> _selectedTourChangedHandler;

        [SetUp]
        public void SetUp()
        {
            // Create mocks for the dependencies
            _mockWpfService = Substitute.For<IWpfService>();
            _mockTourLogService = Substitute.For<ITourLogService>();
            _mockEventAggregator = Substitute.For<IEventAggregator>();
            _mockLogger = Substitute.For<ILogger<TourLogsViewModel>>();

            // Capture the delegate passed to Subscribe
            _mockEventAggregator.Subscribe<SelectedTourChangedEvent>(
                Arg.Do<Action<SelectedTourChangedEvent>>(handler => _selectedTourChangedHandler = handler));

            // Create the ViewModel with the mocked dependencies
            _viewModel = new TourLogsViewModel(_mockWpfService, _mockTourLogService, _mockEventAggregator, _mockLogger);
        }
        
        [Test]
        public void Constructor_WhenWpfServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TourLogsViewModel(null!, _mockTourLogService, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenTourLogServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TourLogsViewModel(_mockWpfService, null!, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenEventAggregatorIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TourLogsViewModel(_mockWpfService, _mockTourLogService, null!, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TourLogsViewModel(_mockWpfService, _mockTourLogService, _mockEventAggregator, null!));
        }

        [Test]
        public void Constructor_SubscribesToSelectedTourChangedEvent()
        {
            // Assert
            _mockEventAggregator.Received(1).Subscribe<SelectedTourChangedEvent>(Arg.Any<Action<SelectedTourChangedEvent>>());
            Assert.That(_selectedTourChangedHandler, Is.Not.Null);
        }
        
        [Test]
        public void OnSelectedTourChanged_WhenEventFires_UpdatesSelectedTourProperty()
        {
            // Arrange
            var newTour = new Tour { TourId = 1, TourName = "Mountain Hike" };
            var tourEvent = new SelectedTourChangedEvent(newTour);
            
            // Act
            // Simulate the event being fired by invoking the captured handler
            _selectedTourChangedHandler.Invoke(tourEvent);
            
            // Assert
            Assert.That(_viewModel.SelectedTour, Is.SameAs(newTour));
        }

        [Test]
        public void ExecuteAddNewTourLog_CanExecute_IsFalse_WhenTourOrNameIsMissing()
        {
            // Arrange
            _viewModel.SelectedTour = null;
            _viewModel.NewLogName = "A log name";
            Assert.IsFalse(_viewModel.ExecuteAddNewTourLog.CanExecute(null));

            _viewModel.SelectedTour = new Tour();
            _viewModel.NewLogName = "";
            Assert.IsFalse(_viewModel.ExecuteAddNewTourLog.CanExecute(null));
        }
        
        [Test]
        public void ExecuteAddNewTourLog_CanExecute_IsTrue_WhenTourAndNameArePresent()
        {
             // Arrange
            _viewModel.SelectedTour = new Tour();
            _viewModel.NewLogName = "A valid log name";
            
            // Assert
            Assert.IsTrue(_viewModel.ExecuteAddNewTourLog.CanExecute(null));
        }

        [Test]
        public void ExecuteDeleteTourLog_CanExecute_IsTrue_WhenLogIsSelected()
        {
            _viewModel.SelectedLog = new TourLog();
            Assert.IsTrue(_viewModel.ExecuteDeleteTourLog.CanExecute(null));
        }
        
        [Test]
        public void ExecuteDeleteTourLog_CanExecute_IsFalse_WhenNoLogIsSelected()
        {
            _viewModel.SelectedLog = null;
            Assert.IsFalse(_viewModel.ExecuteDeleteTourLog.CanExecute(null));
        }
        
        [Test]
        public void ExecuteEditTourLog_CanExecute_IsTrue_WhenTourAndLogAreSelected()
        {
            _viewModel.SelectedTour = new Tour();
            _viewModel.SelectedLog = new TourLog();
            Assert.IsTrue(_viewModel.ExecuteEditTourLog.CanExecute(null));
        }
        
        [Test]
        public void ExecuteEditTourLog_CanExecute_IsFalse_WhenNoTourOrLogIsSelected()
        {
            _viewModel.SelectedTour = null;
            _viewModel.SelectedLog = null;
            Assert.IsFalse(_viewModel.ExecuteEditTourLog.CanExecute(null));
        }
        
        [Test]
        public void ExecuteEditTourLog_CanExecute_IsFalse_WhenOnlyTourIsSelected()
        {
            _viewModel.SelectedTour = new Tour();
            _viewModel.SelectedLog = null;
            Assert.IsFalse(_viewModel.ExecuteEditTourLog.CanExecute(null));
        }
        
        [Test]
        public void AddNewTourLog_WhenExecuted_SpawnsWindowAndClearsName()
        {
            // Arrange
            var tour = new Tour();
            _viewModel.SelectedTour = tour;
            _viewModel.NewLogName = "My first log";

            // Act
            _viewModel.ExecuteAddNewTourLog.Execute(null);

            // Assert
            _mockWpfService.Received(1).SpawnEditTourLogWindow(tour, Arg.Is<TourLog>(log => log.Comment == "My first log"));
            Assert.That(_viewModel.NewLogName, Is.Empty);
        }

        [Test]
        public void EditTourLog_WhenExecuted_SpawnsWindowWithCorrectData()
        {
            // Arrange
            var tour = new Tour();
            var log = new TourLog();
            _viewModel.SelectedTour = tour;
            _viewModel.SelectedLog = log;

            // Act
            _viewModel.ExecuteEditTourLog.Execute(null);

            // Assert
            _mockWpfService.Received(1).SpawnEditTourLogWindow(tour, log);
        }
        
        [Test]
        public async Task DeleteTourLog_WhenSuccessful_DeletesAndRemovesFromLocalTour()
        {
            // Arrange
            var logToDelete = new TourLog { LogId = 101, Comment = "Log to delete" };
            var tour = new Tour { TourId = 1, Logs = new ObservableCollection<TourLog> { logToDelete } };
            _viewModel.SelectedTour = tour;
            _viewModel.SelectedLog = logToDelete;
            _mockTourLogService.DeleteTourLogAsync(101).Returns(Task.FromResult(true));

            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteDeleteTourLog).ExecuteAsync(null);

            // Assert
            await _mockTourLogService.Received(1).DeleteTourLogAsync(101);
            Assert.That(tour.Logs, Does.Not.Contain(logToDelete));
            Assert.That(_viewModel.SelectedLog, Is.Null);
            _mockLogger.Received(1).Info(Arg.Is<string>(s => s.Contains("Deleted log")));
        }
        
        [Test]
        public async Task DeleteTourLog_WhenFails_DoesNotRemoveFromLocalTourAndLogsError()
        {
            // Arrange
            var logToDelete = new TourLog { LogId = 101, Comment = "Log to delete" };
            var tour = new Tour { TourId = 1, Logs = new ObservableCollection<TourLog> { logToDelete } };
            _viewModel.SelectedTour = tour;
            _viewModel.SelectedLog = logToDelete;
            _mockTourLogService.DeleteTourLogAsync(101).Returns(Task.FromResult(false));

            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteDeleteTourLog).ExecuteAsync(null);
            
            // Assert
            await _mockTourLogService.Received(1).DeleteTourLogAsync(101);
            Assert.That(tour.Logs, Contains.Item(logToDelete)); // It should still be there
            Assert.That(_viewModel.SelectedLog, Is.Null); // is set to null regardless of success
            _mockLogger.Received(1).Error(Arg.Is<string>(s => s.Contains("Failed to delete log")));
        }
    }
}
