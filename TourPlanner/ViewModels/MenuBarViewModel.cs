using System.Windows;
using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;

namespace TourPlanner.ViewModels
{
    class MenuBarViewModel : BaseViewModel
    {
        private readonly ILocalTourService _localTourService;
        
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
        public MenuBarViewModel(ILocalTourService localTourService)
        {
            _localTourService = localTourService;
        }
        
        
        // Implementations        
        private async Task ExportTours(object? parameter)
        {
            // Logic to export tours
            MessageBox.Show("Export Tours functionality is not implemented yet.");
        }
        
        private async Task ImportTours(object? parameter)
        {
            // Logic to import tours
            MessageBox.Show("Import Tours functionality is not implemented yet.");
        }
        
        private void ExitApplication(object? parameter)
        {
            // Close the application
            Application.Current.Shutdown();
        }
    }
}
