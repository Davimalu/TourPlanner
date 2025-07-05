using NSubstitute;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Events;
using TourPlanner.ViewModels;
using MessageBoxButton = TourPlanner.Model.Enums.MessageBoxAbstraction.MessageBoxButton;
using MessageBoxImage = TourPlanner.Model.Enums.MessageBoxAbstraction.MessageBoxImage;

namespace TourPlanner.Test.ViewModels
{
    [TestFixture]
    public class MenuBarViewModelTest
    {
        // Mocks for dependencies
        private ILocalTourService _mockLocalTourService;
        private ITourService _mockTourService;
        private IIoService _mockIoService;
        private IPdfService _mockPdfService;
        private IWpfService _mockWpfService;
        private IEventAggregator _mockEventAggregator;
        private ILogger<MenuBarViewModel> _mockLogger;

        // System Under Test (SUT)
        private MenuBarViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            // Create mocks for the dependencies
            _mockLocalTourService = Substitute.For<ILocalTourService>();
            _mockTourService = Substitute.For<ITourService>();
            _mockIoService = Substitute.For<IIoService>();
            _mockPdfService = Substitute.For<IPdfService>();
            _mockWpfService = Substitute.For<IWpfService>();
            _mockEventAggregator = Substitute.For<IEventAggregator>();
            _mockLogger = Substitute.For<ILogger<MenuBarViewModel>>();

            // Initialize the MenuBarViewModel with the mocked dependencies
            _viewModel = new MenuBarViewModel(_mockLocalTourService, _mockTourService, _mockIoService, _mockPdfService, _mockWpfService, _mockEventAggregator, _mockLogger);
        }
        
        [Test]
        public void Constructor_WhenLocalTourServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MenuBarViewModel(null!, _mockTourService, _mockIoService, _mockPdfService, _mockWpfService, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenTourServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MenuBarViewModel(_mockLocalTourService, null!, _mockIoService, _mockPdfService, _mockWpfService, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenIoServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MenuBarViewModel(_mockLocalTourService, _mockTourService, null!, _mockPdfService, _mockWpfService, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenPdfServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MenuBarViewModel(_mockLocalTourService, _mockTourService, _mockIoService, null!, _mockWpfService, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenWpfServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MenuBarViewModel(_mockLocalTourService, _mockTourService, _mockIoService, _mockPdfService, null!, _mockEventAggregator, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenEventAggregatorIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MenuBarViewModel(_mockLocalTourService, _mockTourService, _mockIoService, _mockPdfService, _mockWpfService, null, _mockLogger));
        }
        
        [Test]
        public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MenuBarViewModel(_mockLocalTourService, _mockTourService, _mockIoService, _mockPdfService, _mockWpfService, _mockEventAggregator, null));
        }

        [Test]
        public async Task ExportTours_WhenUserCancelsSaveDialog_DoesNothing()
        {
            // Arrange
            _mockIoService.OpenFileSaveDialog(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(string.Empty);

            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteExportTours).ExecuteAsync(null);

            // Assert
            await _mockTourService.DidNotReceive().GetToursAsync();
            await _mockLocalTourService.DidNotReceive().SaveToursToFileAsync(Arg.Any<List<Tour>>(), Arg.Any<string>());
        }

        [Test]
        public async Task ExportTours_WhenNoToursExist_ShowsInfoMessage()
        {
            // Arrange
            _mockIoService.OpenFileSaveDialog(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("C:\\export.tours");
            _mockTourService.GetToursAsync()!.Returns(Task.FromResult(new List<Tour>()));

            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteExportTours).ExecuteAsync(null);

            // Assert
            _mockWpfService.Received(1).ShowMessageBox("Export Tours", "No tours available to export.", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [Test]
        public async Task ExportTours_AsPdf_WhenSuccessful_CallsPdfServiceAndShowsSuccess()
        {
            // Arrange
            var tours = new List<Tour> { new Tour() };
            _mockIoService.OpenFileSaveDialog(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("C:\\export.pdf");
            _mockTourService.GetToursAsync()!.Returns(Task.FromResult(tours));
            _mockPdfService.ExportToursAsPdfAsync(tours, "C:\\export.pdf").Returns(Task.FromResult(true));

            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteExportTours).ExecuteAsync(null);

            // Assert
            await _mockPdfService.Received(1).ExportToursAsPdfAsync(tours, "C:\\export.pdf");
            _mockWpfService.Received(1).ShowMessageBox("Export Tours", "Tours exported successfully.", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [Test]
        public async Task ExportTours_AsToursFile_WhenSuccessful_CallsLocalTourServiceAndShowsSuccess()
        {
            // Arrange
            var tours = new List<Tour> { new Tour() };
            _mockIoService.OpenFileSaveDialog(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("C:\\export.tours");
            _mockTourService.GetToursAsync()!.Returns(Task.FromResult(tours));
            _mockLocalTourService.SaveToursToFileAsync(Arg.Any<List<Tour>>(), "C:\\export.tours").Returns(Task.FromResult(true));

            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteExportTours).ExecuteAsync(null);

            // Assert
            await _mockLocalTourService.Received(1).SaveToursToFileAsync(Arg.Any<List<Tour>>(), "C:\\export.tours");
            _mockWpfService.Received(1).ShowMessageBox("Export Tours", "Tours exported successfully.", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [Test]
        public async Task ExportTours_WhenExportFails_ShowsErrorMessage()
        {
            // Arrange
            var tours = new List<Tour> { new Tour() };
            _mockIoService.OpenFileSaveDialog(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("C:\\export.tours");
            _mockTourService.GetToursAsync()!.Returns(Task.FromResult(tours));
            _mockLocalTourService.SaveToursToFileAsync(Arg.Any<List<Tour>>(), Arg.Any<string>()).Returns(Task.FromResult(false));

            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteExportTours).ExecuteAsync(null);

            // Assert
            _mockWpfService.Received(1).ShowMessageBox("Export Tours", "Failed to export tours. Please try again.", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        [Test]
        public async Task ExportTours_WithInvalidExtension_ShowsErrorMessage()
        {
            // Arrange
            var tours = new List<Tour> { new Tour() };
            _mockIoService.OpenFileSaveDialog(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("C:\\export.txt");
            _mockTourService.GetToursAsync()!.Returns(Task.FromResult(tours));
            
            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteExportTours).ExecuteAsync(null);

            // Assert
            _mockWpfService.Received(1).ShowMessageBox("Export Tours", "Invalid file format. Please use .tours or .pdf.", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        [Test]
        public async Task ImportTours_WhenUserCancelsOpenDialog_DoesNothing()
        {
            // Arrange
            _mockIoService.OpenFileOpenDialog(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(string.Empty);

            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteImportTours).ExecuteAsync(null);

            // Assert
            await _mockLocalTourService.DidNotReceive().LoadToursFromFileAsync(Arg.Any<string>());
        }

        [Test]
        public async Task ImportTours_WhenFileIsEmptyOrInvalid_ShowsWarningMessage()
        {
            // Arrange
            _mockIoService.OpenFileOpenDialog(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("C:\\import.tours");
            _mockLocalTourService.LoadToursFromFileAsync("C:\\import.tours")!.Returns(Task.FromResult<IEnumerable<Tour>>(new List<Tour>()));

            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteImportTours).ExecuteAsync(null);

            // Assert
            _mockWpfService.Received(1).ShowMessageBox("Import Tours", "No tours found in the selected file.", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        [Test]
        public async Task ImportTours_WhenSuccessful_CreatesToursAndPublishesEvent()
        {
            // Arrange
            var toursToImport = new List<Tour> { new Tour { TourId = 1 }, new Tour { TourId = 2 } };
            _mockIoService.OpenFileOpenDialog(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("C:\\import.tours");
            _mockLocalTourService.LoadToursFromFileAsync("C:\\import.tours")!.Returns(Task.FromResult<IEnumerable<Tour>>(toursToImport));
            _mockTourService.CreateTourAsync(Arg.Any<Tour>()).Returns(Task.FromResult<Tour?>(new Tour())); // Simulate successful creation

            // Act
            await ((RelayCommandAsync)_viewModel.ExecuteImportTours).ExecuteAsync(null);

            // Assert
            await _mockTourService.Received(2).CreateTourAsync(Arg.Any<Tour>());
            _mockEventAggregator.Received(1).Publish(Arg.Is<ToursChangedEvent>(e => e.Tours.SequenceEqual(toursToImport)));
        }

        [Test]
        public void ChangeApplicationTheme_WhenToggleIsChecked_AppliesDarkTheme()
        {
            // Arrange
            _viewModel.ThemeToggleChecked = true;

            // Act
            _viewModel.ExecuteChangeTheme.Execute(null);

            // Assert
            _mockWpfService.Received(1).ApplyDarkTheme();
            _mockWpfService.DidNotReceive().ApplyLightTheme();
        }

        [Test]
        public void ChangeApplicationTheme_WhenToggleIsNotChecked_AppliesLightTheme()
        {
            // Arrange
            _viewModel.ThemeToggleChecked = false;

            // Act
            _viewModel.ExecuteChangeTheme.Execute(null);

            // Assert
            _mockWpfService.Received(1).ApplyLightTheme();
            _mockWpfService.DidNotReceive().ApplyDarkTheme();
        }

        [Test]
        public void ExecuteExitApplication_WhenExecuted_CallsExitApplication()
        {
            // Act
            _viewModel.ExecuteExitApplication.Execute(null);

            // Assert
            _mockWpfService.Received(1).ExitApplication();
        }
    }
}