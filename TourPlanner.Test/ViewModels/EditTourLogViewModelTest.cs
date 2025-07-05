using System.Collections.ObjectModel;
using System.Windows.Input;
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
    public class EditTourLogViewModelTest
    {
        // Mocks for dependencies
        private ITourService _mockTourService;
        private ITourLogService _mockTourLogService;
        private IAttributeService _mockAttributeService;
        private IEventAggregator _mockEventAggregator;
        private ILogger<EditTourLogViewModel> _mockLogger;

        // Sample data for tests
        private Tour _sampleTour;
        private TourLog _sampleTourLog;

        [SetUp]
        public void SetUp()
        {
            // Create mocks for the dependencies
            _mockTourService = Substitute.For<ITourService>();
            _mockTourLogService = Substitute.For<ITourLogService>();
            _mockAttributeService = Substitute.For<IAttributeService>();
            _mockEventAggregator = Substitute.For<IEventAggregator>();
            _mockLogger = Substitute.For<ILogger<EditTourLogViewModel>>();

            // Initialize sample data
            _sampleTour = new Tour { TourId = 1, TourName = "Sample Tour", Logs = new ObservableCollection<TourLog>() };
            _sampleTourLog = new TourLog { LogId = 1, Comment = "Sample Comment" };
        }

        
        /// <summary>
        /// Helper method to create an instance of the EditTourLogViewModel using the mocked dependencies and sample data
        /// </summary>
        /// <param name="tour"></param>
        /// <param name="tourLog"></param>
        /// <returns></returns>
        private EditTourLogViewModel CreateViewModel(Tour tour, TourLog tourLog)
        {
            return new EditTourLogViewModel(tour, tourLog, _mockTourService, _mockTourLogService, 
                                            _mockAttributeService, _mockEventAggregator, _mockLogger);
        }
        
        [Test]
        public void Constructor_WhenTourServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EditTourLogViewModel(_sampleTour, _sampleTourLog, null, _mockTourLogService, _mockAttributeService, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenTourLogServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EditTourLogViewModel(_sampleTour, _sampleTourLog, _mockTourService, null, _mockAttributeService, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenAttributeServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EditTourLogViewModel(_sampleTour, _sampleTourLog, _mockTourService, _mockTourLogService, null, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenEventAggregatorIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EditTourLogViewModel(_sampleTour, _sampleTourLog, _mockTourService, _mockTourLogService, _mockAttributeService, null, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EditTourLogViewModel(_sampleTour, _sampleTourLog, _mockTourService, _mockTourLogService, _mockAttributeService, _mockEventAggregator, null));
        }

        [Test]
        public void ExecuteSave_CanExecute_IsFalse_WhenCommentIsEmpty()
        {
            // Arrange
            _sampleTourLog.Comment = "";
            var viewModel = CreateViewModel(_sampleTour, _sampleTourLog);

            // Act & Assert
            Assert.IsFalse(viewModel.ExecuteSave.CanExecute(null));
        }
        
        [Test]
        public void ExecuteSave_CanExecute_IsTrue_WhenCommentIsNotEmpty()
        {
            // Arrange
            _sampleTourLog.Comment = "Valid comment";
            var viewModel = CreateViewModel(_sampleTour, _sampleTourLog);

            // Act & Assert
            Assert.IsTrue(viewModel.ExecuteSave.CanExecute(null));
        }

        [Test]
        public void ExecuteCancel_WhenExecuted_PublishesCloseWindowRequest()
        {
            // Arrange
            var viewModel = CreateViewModel(_sampleTour, _sampleTourLog);

            // Act
            viewModel.ExecuteCancel.Execute(null);

            // Assert
            _mockEventAggregator.Received(1).Publish(Arg.Is<CloseWindowRequestedEvent>(e => e.DataContextOfWindowToClose == viewModel));
        }

        [Test]
        public async Task ExecuteSave_WhenCreatingNewLog_CallsCreateAndUpdatesTour()
        {
            // Arrange
            var newLogTemplate = new TourLog { LogId = 0, Comment = "New Log" };
            var createdLogFromDb = new TourLog { LogId = 99, Comment = "New Log" }; // The log after DB insertion
            var viewModel = CreateViewModel(_sampleTour, newLogTemplate);

            _mockTourLogService.CreateTourLogAsync(_sampleTour.TourId, viewModel.EditableTourLog).Returns(Task.FromResult<TourLog?>(createdLogFromDb));
            _mockAttributeService.CalculatePopularityAsync(Arg.Any<Tour>()).Returns(50f);
            _mockAttributeService.CalculateChildFriendliness(Arg.Any<Tour>()).Returns(75f);

            // Act
            await ((RelayCommandAsync)viewModel.ExecuteSave).ExecuteAsync(null);

            // Assert - Log creation
            await _mockTourLogService.Received(1).CreateTourLogAsync(_sampleTour.TourId, viewModel.EditableTourLog);
            await _mockTourLogService.DidNotReceive().UpdateTourLogAsync(Arg.Any<TourLog>());
            Assert.That(_sampleTour.Logs, Contains.Item(createdLogFromDb));

            // Assert - Attribute calculation and Tour update
            await _mockAttributeService.Received(1).CalculatePopularityAsync(_sampleTour);
            _mockAttributeService.Received(1).CalculateChildFriendliness(_sampleTour);
            await _mockTourService.Received(1).UpdateTourAsync(_sampleTour);

            // Assert - Window close
            _mockEventAggregator.Received(1).Publish(Arg.Any<CloseWindowRequestedEvent>());
        }

        [Test]
        public async Task ExecuteSave_WhenUpdatingExistingLog_CallsUpdateAndUpdatesTour()
        {
            // Arrange
            var existingLog = new TourLog { LogId = 1, Comment = "Old Comment" };
            _sampleTour.Logs.Add(existingLog);
            var editedLog = new TourLog(existingLog) { Comment = "Updated Comment" };
            var viewModel = CreateViewModel(_sampleTour, editedLog);
            
            var updatedLogFromDb = new TourLog { LogId = 1, Comment = "Updated Comment From DB" };

            _mockTourLogService.UpdateTourLogAsync(viewModel.EditableTourLog).Returns(Task.FromResult<TourLog?>(updatedLogFromDb));

            // Act
            await ((RelayCommandAsync)viewModel.ExecuteSave).ExecuteAsync(null);

            // Assert - Log update
            await _mockTourLogService.Received(1).UpdateTourLogAsync(viewModel.EditableTourLog);
            await _mockTourLogService.DidNotReceive().CreateTourLogAsync(Arg.Any<int>(), Arg.Any<TourLog>());
            Assert.That(_sampleTour.Logs.First(l => l.LogId == 1), Is.SameAs(updatedLogFromDb)); // Check if the instance was replaced

            // Assert - Attribute calculation and Tour update
            await _mockAttributeService.Received(1).CalculatePopularityAsync(_sampleTour);
            await _mockTourService.Received(1).UpdateTourAsync(_sampleTour);
        }

        [Test]
        public async Task ExecuteSave_WhenLogServiceFails_LogsErrorAndDoesNotUpdateAttributes()
        {
            // Arrange
            var viewModel = CreateViewModel(_sampleTour, _sampleTourLog);
            _mockTourLogService.UpdateTourLogAsync(Arg.Any<TourLog>()).Returns(Task.FromResult<TourLog?>(null));
            _sampleTour.Logs.Add(_sampleTourLog);
            
            // Act
            await ((RelayCommandAsync)viewModel.ExecuteSave).ExecuteAsync(null);
            
            // Assert
            _mockLogger.Received(1).Error(Arg.Is<string>(s => s.Contains("Failed to update TourLog")));
            
            // Crucially, verify that the subsequent steps were NOT taken
            await _mockAttributeService.DidNotReceive().CalculatePopularityAsync(Arg.Any<Tour>());
            _mockAttributeService.DidNotReceive().CalculateChildFriendliness(Arg.Any<Tour>());
            await _mockTourService.DidNotReceive().UpdateTourAsync(Arg.Any<Tour>());
        }
    }
}