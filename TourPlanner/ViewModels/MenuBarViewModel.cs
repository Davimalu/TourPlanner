using System.Net;
using System.Windows;
using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model.Exceptions;

namespace TourPlanner.ViewModels
{
    class MenuBarViewModel : BaseViewModel
    {
        private readonly ILocalTourService _localTourService;
        private readonly ITourService _tourService;
        private readonly IIoService _ioService;
        private readonly ILoggerWrapper _logger;
        
        // Commands
        private RelayCommandAsync? _executeExportTours;
        private RelayCommandAsync? _executeImportTours;
        private RelayCommand? _executeExitApplication;
        
        public ICommand ExecuteExportTours => _executeExportTours ??= 
            new RelayCommandAsync(ExportTours, _ => true);
        
        public ICommand ExecuteImportTours => _executeImportTours ??= 
            new RelayCommandAsync(ImportTours, _ => true);

        public RelayCommand? ExecuteExitApplication => _executeExitApplication ??= 
            new RelayCommand(ExitApplication);
        
        
        // Constructor
        public MenuBarViewModel(ILocalTourService localTourService, ITourService tourService, IIoService ioService)
        {
            _localTourService = localTourService ?? throw new ArgumentNullException(nameof(localTourService));
            _tourService = tourService ?? throw new ArgumentNullException(nameof(tourService));
            _ioService = ioService ?? throw new ArgumentNullException(nameof(ioService));
            
            _logger = LoggerFactory.GetLogger<MenuBarViewModel>();
        }
        
        
        // TODO: Create MessageBoxService to handle messages in a more consistent way
        
        // Implementations        
        private async Task ExportTours(object? parameter)
        {
            // Get the save path from the user
            string savePath = _ioService.OpenFileSaveDialog("Tour Files (*.tours)|*.tours", "Export Tours", "%userprofile%");
            
            if (string.IsNullOrEmpty(savePath))
            {
                // User cancelled the save dialog
                return;
            }

            // Get all tours from the service
            var tours = await _tourService.GetToursAsync();
            if (tours == null || tours.Count == 0)
            {
                MessageBox.Show("No tours available to export.", "Export Tours", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            // Export tours to the specified file
            _logger.Debug($"Exporting {tours.Count} tours to {savePath}");
            var success = await _localTourService.SaveToursToFileAsync(tours.ToList(), savePath);
            
            // Check if the export was successful
            if (success)
            {
                _logger.Info($"Tours exported successfully to {savePath}");
                MessageBox.Show("Tours exported successfully.", "Export Tours", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                _logger.Error($"Failed to export tours to {savePath}");
                MessageBox.Show("Failed to export tours. Please try again.", "Export Tours", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async Task ImportTours(object? parameter)
        {
            // Get the file path from the user
            string loadPath = _ioService.OpenFileOpenDialog("Tour Files (*.tours)|*.tours", "Import Tours", "%userprofile%");
            
            if (string.IsNullOrEmpty(loadPath))
            {
                // User cancelled the open dialog
                return;
            }
            
            // Load tours from the specified file
            var tours = await _localTourService.LoadToursFromFileAsync(loadPath);
            
            // Check if tours were loaded successfully
            if (tours == null || !tours.Any())
            {
                MessageBox.Show("No tours found in the selected file.", "Import Tours", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            _logger.Debug($"Importing {tours.Count()} tours from {loadPath}");
            
            // Save the imported tours to the database
            foreach (var tour in tours)
            {
                // Check if the tour already exists
                try
                {
                    await _tourService.GetTourByIdAsync(tour.TourId);
                }
                catch (ApiServiceException ex)
                {
                    if (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        // Tour does not exist, proceed with import
                        _logger.Debug($"Tour with ID {tour.TourId} does not exist. Proceeding with import.");
                        
                        // Create the tour in the service
                        var createdTour = await _tourService.CreateTourAsync(tour);
                        if (createdTour == null)
                        {
                            _logger.Error($"Failed to import tour: {tour.TourName}");
                            MessageBox.Show($"Failed to import tour: {tour.TourName}", "Import Tours", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        _logger.Error($"Error checking tour existence: {ex.Message}");
                        MessageBox.Show($"Error checking tour existence: {ex.Message}", "Import Tours", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                // Tour already exists, do nothing
            }
        }
        
        private void ExitApplication(object? parameter)
        {
            // Close the application
            Application.Current.Shutdown();
        }
    }
}
