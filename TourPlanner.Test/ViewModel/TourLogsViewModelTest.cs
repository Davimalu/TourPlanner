using NSubstitute;
using System.Collections.ObjectModel;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Models;
using TourPlanner.ViewModels;

namespace TourPlanner.Test.ViewModel
{
    class TourLogsViewModelTest
    {
        private ISelectedTourService _selectedTourService;
        private TourLogsViewModel _tourLogsViewModel;

        [SetUp]
        public void Setup()
        {
            _selectedTourService = Substitute.For<ISelectedTourService>();
            _tourLogsViewModel = new TourLogsViewModel(_selectedTourService);
        }

        [Test]
        public void ExecuteAddNewTourLog_ButtonInactiveWhenNoSelectedTour()
        {
            // Arrange: Set no selected tour and a valid NewLogName
            _tourLogsViewModel.SelectedTour = null;
            _tourLogsViewModel.NewLogName = "Test Log";

            // Act & Assert: The command should not be executable
            Assert.IsFalse(_tourLogsViewModel.ExecuteAddNewTourLog.CanExecute(null));
        }

        [Test]
        public void ExecuteAddNewTourLog_ButtonInactiveWhenNewLogNameEmpty()
        {
            // Arrange: Set a valid tour but an empty NewLogName
            var tour = new Tour { Logs = new ObservableCollection<TourLog>() };
            _tourLogsViewModel.SelectedTour = tour;
            _tourLogsViewModel.NewLogName = "";

            // Act & Assert: The command should not be executable
            Assert.IsFalse(_tourLogsViewModel.ExecuteAddNewTourLog.CanExecute(null));
        }

        [Test]
        public void ExecuteAddNewTourLog_AddsTourLog()
        {
            // Arrange: Create a valid tour and set the NewLogName
            var tour = new Tour { Logs = new ObservableCollection<TourLog>() };
            _tourLogsViewModel.SelectedTour = tour;
            _tourLogsViewModel.NewLogName = "Test Log";

            // Act: Execute the add command
            _tourLogsViewModel.ExecuteAddNewTourLog.Execute(null);

            // Assert: The newly added log has the correct comment
            Assert.IsTrue(tour.Logs.Any(l => l.Comment == "Test Log"));
        }

        [Test]
        public void ExecuteAddNewTourLog_ClearsNewLogName()
        {
            // Arrange: Create a valid tour and set the NewLogName
            var tour = new Tour { Logs = new ObservableCollection<TourLog>() };
            _tourLogsViewModel.SelectedTour = tour;
            _tourLogsViewModel.NewLogName = "Test Log";
            int initialCount = tour.Logs.Count;

            // Act: Execute the add command
            _tourLogsViewModel.ExecuteAddNewTourLog.Execute(null);

            // Assert: The NewLogName property is cleared
            Assert.IsTrue(string.IsNullOrEmpty(_tourLogsViewModel.NewLogName));
        }

        [Test]
        public void ExecuteDeleteTourLog_ButtonInactiveWhenNoSelectedTour()
        {
            // Arrange: Set SelectedTour to null but assign a dummy SelectedLog
            _tourLogsViewModel.SelectedTour = null;
            _tourLogsViewModel.SelectedLog = new TourLog { Comment = "Log" };

            // Act & Assert: Command should not be executable
            Assert.IsFalse(_tourLogsViewModel.ExecuteDeleteTourLog.CanExecute(null));
        }

        [Test]
        public void ExecuteDeleteTourLog_ButtonInactiveWhenNoSelectedLog()
        {
            // Arrange: Set a valid SelectedTour but leave SelectedLog as null
            var tour = new Tour { Logs = new ObservableCollection<TourLog>() };
            _tourLogsViewModel.SelectedTour = tour;
            _tourLogsViewModel.SelectedLog = null;

            // Act & Assert
            Assert.IsFalse(_tourLogsViewModel.ExecuteDeleteTourLog.CanExecute(null));
        }

        [Test]
        public void ExecuteDeleteTourLog_RemovesSelectedLog()
        {
            // Arrange: Create a tour with a log
            var tour = new Tour { Logs = new ObservableCollection<TourLog>() };
            var log = new TourLog { Comment = "Log to delete" };
            tour.Logs.Add(log);

            // Set the SelectedTour and SelectedLog properties
            _tourLogsViewModel.SelectedTour = tour;
            _tourLogsViewModel.SelectedLog = log;

            // Act: Execute the delete command
            _tourLogsViewModel.ExecuteDeleteTourLog.Execute(null);

            // Assert: The log is removed
            Assert.IsFalse(tour.Logs.Contains(log));
        }

        [Test]
        public void ExecuteDeleteTourLog_ResetsSelectedLog()
        {
            // Arrange: Create a tour with a log
            var tour = new Tour { Logs = new ObservableCollection<TourLog>() };
            var log = new TourLog { Comment = "Log to delete" };
            tour.Logs.Add(log);

            // Set the SelectedTour and SelectedLog properties
            _tourLogsViewModel.SelectedTour = tour;
            _tourLogsViewModel.SelectedLog = log;

            // Act: Execute the delete command
            _tourLogsViewModel.ExecuteDeleteTourLog.Execute(null);

            // Assert: The SelectedLog property is reset to null.
            Assert.IsNull(_tourLogsViewModel.SelectedLog);
        }

        [Test]
        public void ExecuteEditTourLog_ButtonInactiveWhenNoSelectedTour()
        {
            // Arrange: SelectedTour is null but SelectedLog is set
            _tourLogsViewModel.SelectedTour = null;
            _tourLogsViewModel.SelectedLog = new TourLog { Comment = "Log" };

            // Act & Assert
            Assert.IsFalse(_tourLogsViewModel.ExecuteEditTourLog.CanExecute(null));
        }

        [Test]
        public void ExecuteEditTourLog_ButtonInactiveWhenNoSelectedLog()
        {
            // Arrange: Set a valid SelectedTour but SelectedLog is null
            var tour = new Tour { Logs = new ObservableCollection<TourLog>() };
            _tourLogsViewModel.SelectedTour = tour;
            _tourLogsViewModel.SelectedLog = null;

            // Act & Assert
            Assert.IsFalse(_tourLogsViewModel.ExecuteEditTourLog.CanExecute(null));
        }

        [Test]
        public void ExecuteEditTourLog_ButtonActiveWhenBothSelectedTourAndSelectedLogAreSet()
        {
            // Arrange: Create a tour with at least one log
            var tour = new Tour { Logs = new ObservableCollection<TourLog>() };
            var log = new TourLog { Comment = "Editable Log" };
            tour.Logs.Add(log);

            // Set the SelectedTour and SelectedLog properties
            _tourLogsViewModel.SelectedTour = tour;
            _tourLogsViewModel.SelectedLog = log;

            // Act & Assert: Command should be executable
            Assert.IsTrue(_tourLogsViewModel.ExecuteEditTourLog.CanExecute(null));
        }
    }
}
