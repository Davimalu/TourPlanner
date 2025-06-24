using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model.Events;
using MessageBoxButton = TourPlanner.Model.Enums.MessageBoxAbstraction.MessageBoxButton;
using MessageBoxImage = TourPlanner.Model.Enums.MessageBoxAbstraction.MessageBoxImage;

namespace TourPlanner.ViewModels
{
    class MenuBarViewModel : BaseViewModel
    {
        private readonly ILocalTourService _localTourService;
        private readonly ITourService _tourService;
        private readonly IIoService _ioService;
        private readonly IPdfService _pdfService;
        private readonly IWpfService _wpfService;
        private readonly ILogger<MenuBarViewModel> _logger;
        
        // UI Elements
        private bool _themeToggleChecked;
        public bool ThemeToggleChecked
        {
            get => _themeToggleChecked;
            set
            {
                _themeToggleChecked = value;
                RaisePropertyChanged(nameof(ThemeToggleChecked));
            }
        }
        
        // Commands
        private RelayCommandAsync? _executeExportTours;
        private RelayCommandAsync? _executeImportTours;
        private RelayCommand _executeChangeTheme;
        private RelayCommand? _executeExitApplication;
        
        public ICommand ExecuteExportTours => _executeExportTours ??= 
            new RelayCommandAsync(ExportTours, _ => true);
        
        public ICommand ExecuteImportTours => _executeImportTours ??= 
            new RelayCommandAsync(ImportTours, _ => true);
        
        public RelayCommand ExecuteChangeTheme => _executeChangeTheme ??=
            new RelayCommand(ChangeApplicationTheme, _ => true);

        public RelayCommand? ExecuteExitApplication => _executeExitApplication ??=
            new RelayCommand(_ => _wpfService.ExitApplication());
        
        
        // Constructor
        public MenuBarViewModel(ILocalTourService localTourService, ITourService tourService, IIoService ioService, IPdfService pdfService, IWpfService wpfService, IEventAggregator eventAggregator, ILogger<MenuBarViewModel> logger) : base(eventAggregator)
        {
            _localTourService = localTourService ?? throw new ArgumentNullException(nameof(localTourService));
            _tourService = tourService ?? throw new ArgumentNullException(nameof(tourService));
            _ioService = ioService ?? throw new ArgumentNullException(nameof(ioService));
            _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
            _wpfService = wpfService ?? throw new ArgumentNullException(nameof(wpfService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Exports all tours to a file or PDF, depending on the user's choice
        /// </summary>
        /// <param name="parameter"></param>
        private async Task ExportTours(object? parameter)
        {
            // Get the save path from the user
            string savePath = _ioService.OpenFileSaveDialog("Tour Files (*.tours)|*.tours|PDF Files (*.pdf)|*.pdf", "Export Tours", "%userprofile%");
            
            if (string.IsNullOrEmpty(savePath))
            {
                // User cancelled the save dialog
                return;
            }

            // Get all tours from the service
            var tours = await _tourService.GetToursAsync();
            if (tours == null || tours.Count == 0)
            {
                _wpfService.ShowMessageBox("Export Tours", "No tours available to export.", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            // Determine the output format based on the file extension
            var fileExtension = System.IO.Path.GetExtension(savePath).ToLowerInvariant();
            bool success = false;
            
            // If the user selected PDF, create a PDF Summary
            if (fileExtension == ".pdf")
            {
                _logger.Debug($"Exporting {tours.Count} tours as PDF to {savePath}");
                success = await _pdfService.ExportToursAsPdfAsync(tours, savePath);
            } else if (fileExtension == ".tours")
            {
                // Export tours to the specified file
                _logger.Debug($"Exporting {tours.Count} tours to {savePath}");
                success = await _localTourService.SaveToursToFileAsync(tours.ToList(), savePath);
            }
            else
            {
                _wpfService.ShowMessageBox("Export Tours", "Invalid file format. Please use .tours or .pdf.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Check if the export was successful
            if (success)
            {
                _logger.Info($"Tours exported successfully to {savePath}");
                _wpfService.ShowMessageBox("Export Tours", "Tours exported successfully.", MessageBoxButton.OK, MessageBoxImage.Information);
                
            }
            else
            {
                _logger.Error($"Failed to export tours to {savePath}");
                _wpfService.ShowMessageBox("Export Tours", "Failed to export tours. Please try again.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        
        /// <summary>
        /// Imports tours from a file and saves them to the database
        /// </summary>
        /// <param name="parameter"></param>
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
            var tours = (await _localTourService.LoadToursFromFileAsync(loadPath))?.ToList();
            
            // Check if tours were loaded successfully
            if (tours == null || !tours.Any())
            {
                _wpfService.ShowMessageBox("Import Tours", "No tours found in the selected file.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            _logger.Debug($"Importing {tours.Count()} tours from {loadPath}");
            
            // Save the imported tours to the database
            foreach (var tour in tours)
            {
                _logger.Info($"Importing Tour {tour.TourId}: {tour.TourName} | {tour.TourDescription}...");
                
                // Create the tour in the database
                var createdTour = await _tourService.CreateTourAsync(tour);
                if (createdTour == null)
                {
                    _logger.Error($"Failed to import tour {tour.TourId}: {tour.TourName}");
                    _wpfService.ShowMessageBox("Import Tours", $"Failed to import tour {tour.TourId}: {tour.TourName}. Please check the logs for more details.", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    _logger.Info($"Successfully imported tour {tour.TourId}: {createdTour.TourName}");
                }
            }
            
            // Inform the UI there may be new tours
            EventAggregator.Publish(new ToursChangedEvent(tours.ToList()));
        }


        /// <summary>
        /// Changes the application theme based on the toggle state
        /// </summary>
        /// <param name="parameter"></param>
        private void ChangeApplicationTheme(object? parameter)
        {
            // Toggle the theme based on the current state
            if (ThemeToggleChecked)
            {
                _wpfService.ApplyDarkTheme();
                _logger.Info("Changed application theme to Dark");
            }
            else
            {
                _wpfService.ApplyLightTheme();
                _logger.Info("Changed application theme to Light");
            }
        }
    }
}
