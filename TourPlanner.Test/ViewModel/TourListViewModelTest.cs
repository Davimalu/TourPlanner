using NSubstitute;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.ViewModels;

namespace TourPlanner.Test.ViewModel
{
    public class TourListViewModelTest
    {
        private TourListViewModel _tourListViewModel;
        private ITourService _tourService;
        private IUiService _uiService;

        [SetUp]
        public void Setup()
        {
            _tourService = Substitute.For<ITourService>();
            _uiService = Substitute.For<IUiService>();
            _tourListViewModel = new TourListViewModel(_tourService, _uiService);
        }

        [Test]
        public void ExecuteAddNewTour_ButtonInactiveOnEmptyTextbox()
        {
            // Arrange
            _tourListViewModel.NewTourName = "";

            // Act & Assert
            Assert.IsFalse(_tourListViewModel.ExecuteAddNewTour.CanExecute(null));
        }

        [Test]
        public void ExecuteAddNewTour_ButtonActiveOnFilledTextbox()
        {
            // Arrange
            _tourListViewModel.NewTourName = "Test Tour";

            // Act & Assert
            Assert.IsTrue(_tourListViewModel.ExecuteAddNewTour.CanExecute(null));
        }

        [Test]
        public void ExecuteAddNewTour_TourAddedToList()
        {
            // Arrange
            _tourListViewModel.NewTourName = "Test Tour";

            // Act
            _tourListViewModel.ExecuteAddNewTour.Execute(null);

            // Assert
            Assert.IsNotNull(_tourListViewModel.Tours);
            Assert.IsTrue(_tourListViewModel.Tours.Any(t => t.TourName == "Test Tour"));
        }

        [Test]
        public void ExecuteAddNewTour_NewTourNameClearedAfterAdd()
        {
            // Arrange
            _tourListViewModel.NewTourName = "Test Tour";

            // Act
            _tourListViewModel.ExecuteAddNewTour.Execute(null);

            // Assert
            Assert.IsTrue(string.IsNullOrEmpty(_tourListViewModel.NewTourName));
        }

        [Test]
        public void ExecuteDeleteTour_ButtonInactiveWhenNoTourSelected()
        {
            // Arrange
            _tourListViewModel.SelectedTour = null;

            // Act & Assert
            Assert.IsFalse(_tourListViewModel.ExecuteDeleteTour.CanExecute(null));
        }

        [Test]
        public void ExecuteDeleteTour_ButtonActiveWhenTourSelected()
        {
            // Arrange
            _tourListViewModel.SelectedTour = new Tour();

            // Act & Assert
            Assert.IsTrue(_tourListViewModel.ExecuteDeleteTour.CanExecute(null));
        }

        [Test]
        public void ExecuteDeleteTour_TourDeletedFromList()
        {
            // Arrange
            var tour = new Tour();
            _tourListViewModel.Tours?.Add(tour);
            _tourListViewModel.SelectedTour = tour;

            // Act
            _tourListViewModel.ExecuteDeleteTour.Execute(null);

            // Assert
            Assert.IsNotNull(_tourListViewModel.Tours);
            Assert.IsFalse(_tourListViewModel.Tours.Contains(tour));
        }

        [Test]
        public void ExecuteEditTour_ButtonInactiveWhenNoTourSelected()
        {
            // Arrange
            _tourListViewModel.SelectedTour = null;

            // Act & Assert
            Assert.IsFalse(_tourListViewModel.ExecuteEditTour.CanExecute(null));
        }

        [Test]
        public void ExecuteEditTour_ButtonActiveWhenTourSelected()
        {
            // Arrange
            _tourListViewModel.SelectedTour = new Tour() { TourName = "Edit Tour" };

            // Act & Assert
            Assert.IsTrue(_tourListViewModel.ExecuteEditTour.CanExecute(null));
        }

        [Test]
        public void SettingSelectedTour_UpdatesSelectedTourService()
        {
            // Arrange
            var tour = new Tour() { TourName = "Test Selected Tour" };

            // Act
            _tourListViewModel.SelectedTour = tour;

            // Assert: Verify that the service's SelectedTour property was set accordingly
            _selectedTourService.Received(1).SelectedTour = tour;
        }
    }
}