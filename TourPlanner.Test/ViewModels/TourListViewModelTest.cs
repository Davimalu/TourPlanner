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
    public class TourListViewModelTest
    {
        // Mocks
        private IWpfService _mockWpfService;
        private ITourService _mockTourService;
        private ISearchService _mockSearchService;
        private IIoService _mockIoService;
        private IPdfService _mockPdfService;
        private IEventAggregator _mockEventAggregator;
        private ILogger<TourListViewModel> _mockLogger;

        // SUT
        private TourListViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            // Create mocks for the dependencies
            _mockWpfService = Substitute.For<IWpfService>();
            _mockTourService = Substitute.For<ITourService>();
            _mockSearchService = Substitute.For<ISearchService>();
            _mockIoService = Substitute.For<IIoService>();
            _mockPdfService = Substitute.For<IPdfService>();
            _mockEventAggregator = Substitute.For<IEventAggregator>();
            _mockLogger = Substitute.For<ILogger<TourListViewModel>>();

            // Set up the TourService to return an empty list of tours
            _mockTourService.GetToursAsync()!.Returns(Task.FromResult(new List<Tour>()));

            _viewModel = new TourListViewModel(_mockTourService, _mockWpfService, _mockSearchService, _mockIoService, _mockPdfService, _mockEventAggregator, _mockLogger);
        }
        
        [Test]
        public void Constructor_WhenTourServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TourListViewModel(null!, _mockWpfService, _mockSearchService, _mockIoService, _mockPdfService, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenWpfServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TourListViewModel(_mockTourService, null!, _mockSearchService, _mockIoService, _mockPdfService, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenSearchServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TourListViewModel(_mockTourService, _mockWpfService, null!, _mockIoService, _mockPdfService, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenIoServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TourListViewModel(_mockTourService, _mockWpfService, _mockSearchService, null!, _mockPdfService, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenPdfServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TourListViewModel(_mockTourService, _mockWpfService, _mockSearchService, _mockIoService, null!, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenEventAggregatorIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TourListViewModel(_mockTourService, _mockWpfService, _mockSearchService, _mockIoService, _mockPdfService, null!, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TourListViewModel(_mockTourService, _mockWpfService, _mockSearchService, _mockIoService, _mockPdfService, _mockEventAggregator, null!));
        }

        [Test]
        public void Constructor_SubscribesToEvents()
        {
            _mockEventAggregator.Received(1).Subscribe<SearchQueryChangedEvent>(Arg.Any<Action<SearchQueryChangedEvent>>());
            _mockEventAggregator.Received(1).Subscribe<ToursChangedEvent>(Arg.Any<Action<ToursChangedEvent>>());
        }

        [Test]
        public void Constructor_InitiatesTourLoad()
        {
            // Act and Assert
            _mockTourService.Received(1).GetToursAsync();
        }

        [Test]
        public void ExecuteAddNewTour_CanExecute_IsTrue_WhenNewTourNameIsNotEmpty()
        {
            _viewModel.NewTourName = "A new Tour";
            Assert.IsTrue(_viewModel.ExecuteAddNewTour.CanExecute(null));
        }

        [TestCase(null)]
        [TestCase("")]
        public void ExecuteAddNewTour_CanExecute_IsFalse_WhenNewTourNameIsNullOrEmpty(string? name)
        {
            _viewModel.NewTourName = name;
            Assert.IsFalse(_viewModel.ExecuteAddNewTour.CanExecute(null));
        }

        [Test]
        public void ExecuteDeleteTour_CanExecute_IsTrue_WhenTourIsSelected()
        {
            _viewModel.SelectedTour = new Tour();
            Assert.IsTrue(_viewModel.ExecuteDeleteTour.CanExecute(null));
        }
        
        [Test]
        public void ExecuteDeleteTour_CanExecute_IsFalse_WhenNoTourIsSelected()
        {
            _viewModel.SelectedTour = null;
            Assert.IsFalse(_viewModel.ExecuteDeleteTour.CanExecute(null));
        }
        
        [Test]
        public void ExecuteEditTour_CanExecute_IsTrue_WhenTourIsSelected()
        {
            _viewModel.SelectedTour = new Tour();
            Assert.IsTrue(_viewModel.ExecuteEditTour.CanExecute(null));
        }
        
        [Test]
        public void ExecuteEditTour_CanExecute_IsFalse_WhenNoTourIsSelected()
        {
            _viewModel.SelectedTour = null;
            Assert.IsFalse(_viewModel.ExecuteEditTour.CanExecute(null));
        }
        
        [Test]
        public void ExecuteExportTour_CanExecute_IsTrue_WhenTourIsSelected()
        {
            _viewModel.SelectedTour = new Tour();
            Assert.IsTrue(_viewModel.ExecuteExportTour.CanExecute(null));
        }
        
        [Test]
        public void ExecuteExportTour_CanExecute_IsFalse_WhenNoTourIsSelected()
        {
            _viewModel.SelectedTour = null;
            Assert.IsFalse(_viewModel.ExecuteExportTour.CanExecute(null));
        }

        [Test]
        public async Task ExecuteAddNewTour_SpawnsWindowAndClearsName()
        {
            // Arrange
            _viewModel.NewTourName = "My New Adventure";

            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteAddNewTour).ExecuteAsync(null);

            // Assert
            _mockWpfService.Received(1).SpawnEditTourWindow(Arg.Is<Tour>(t => t.TourName == "My New Adventure" && t.TourId == -1));
            Assert.That(_viewModel.NewTourName, Is.Empty);
            await _mockTourService.Received(2).GetToursAsync(); // Once in constructor, once in AddNewTour
        }

        [Test]
        public async Task ExecuteDeleteTour_WhenSuccessful_RemovesTourAndPublishesEvent()
        {
            // Arrange
            var tourToDelete = new Tour { TourId = 1, TourName = "Old Tour" };
            _viewModel.Tours = new ObservableCollection<Tour> { tourToDelete, new Tour { TourId = 2 } };
            _viewModel.SelectedTour = tourToDelete;
            _mockTourService.DeleteTourAsync(1).Returns(Task.FromResult(true));

            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteDeleteTour).ExecuteAsync(null);

            // Assert
            Assert.That(_viewModel.Tours.Count, Is.EqualTo(1));
            Assert.That(_viewModel.Tours.All(t => t.TourId != 1), Is.True);
            await _mockTourService.Received(1).DeleteTourAsync(1);
            Assert.That(_viewModel.SelectedTour, Is.Null);
            _mockEventAggregator.Received(1).Publish(Arg.Any<ToursChangedEvent>());
        }

        [Test]
        public async Task ExecuteEditTour_SpawnsEditWindow()
        {
            // Arrange
            var tourToEdit = new Tour { TourId = 1 };
            _viewModel.SelectedTour = tourToEdit;

            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteEditTour).ExecuteAsync(null);

            // Assert
            _mockWpfService.Received(1).SpawnEditTourWindow(tourToEdit);
            await _mockTourService.Received(2).GetToursAsync(); // Once in constructor, once in EditExistingTour
        }
        
        [Test]
        public async Task ExecuteExportTour_WhenSuccessful_CallsPdfService()
        {
            // Arrange
            var tourToExport = new Tour { TourId = 1, TourName = "Export Me" };
            _viewModel.SelectedTour = tourToExport;
            _mockIoService.OpenFileSaveDialog(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("C:\\export.pdf");
            _mockPdfService.ExportTourAsPdfAsync(tourToExport, "C:\\export.pdf").Returns(Task.FromResult(true));

            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteExportTour).ExecuteAsync(null);
            
            // Assert
            await _mockPdfService.Received(1).ExportTourAsPdfAsync(tourToExport, "C:\\export.pdf");
        }
    }
}
