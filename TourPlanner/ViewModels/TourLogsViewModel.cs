using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Events;

namespace TourPlanner.ViewModels
{
    public class TourLogsViewModel : BaseViewModel
    {
        // Dependencies
        private readonly IWpfService _wpfService;
        private readonly ITourLogService _tourLogService;
        private readonly ILogger<TourLogsViewModel> _logger;
        
        // Commands
        private RelayCommand? _executeAddNewTourLog;
        private RelayCommandAsync? _executeDeleteTourLog;
        private RelayCommand? _executeEditTourLog;
        
        public ICommand ExecuteAddNewTourLog => _executeAddNewTourLog ??= 
            new RelayCommand(AddNewTourLog, _ => SelectedTour != null && !string.IsNullOrEmpty(NewLogName));
        
        public ICommand ExecuteDeleteTourLog => _executeDeleteTourLog ??= 
            new RelayCommandAsync(DeleteTourLog, _ => SelectedLog != null);
        
        public ICommand ExecuteEditTourLog => _executeEditTourLog ??= 
            new RelayCommand(EditTourLog, _ => SelectedLog != null && SelectedTour != null);

        // UI Bindings
        private string? _newLogName;
        public string? NewLogName
        {
            get => _newLogName;
            set
            {
                _newLogName = value;
                RaisePropertyChanged(nameof(NewLogName));
                
                // Notify the command that its execution state may have changed
                _executeAddNewTourLog?.RaiseCanExecuteChanged();
            }
        }
        
        private Tour? _selectedTour;
        public Tour? SelectedTour
        {
            get => _selectedTour;
            set
            {
                _selectedTour = value;
                RaisePropertyChanged(nameof(SelectedTour));
            }
        }
        
        private TourLog? _selectedLog;
        public TourLog? SelectedLog
        {
            get => _selectedLog;
            set
            {
                _selectedLog = value;
                RaisePropertyChanged(nameof(SelectedLog));
                
                // Notify the commands that their execution state may have changed
                _executeDeleteTourLog?.RaiseCanExecuteChanged();
                _executeEditTourLog?.RaiseCanExecuteChanged();
            }
        }


        public TourLogsViewModel(IWpfService wpfService, ITourLogService tourLogService, IEventAggregator eventAggregator, ILogger<TourLogsViewModel> logger) : base(eventAggregator)
        {
            _wpfService = wpfService ?? throw new ArgumentNullException(nameof(wpfService));
            _tourLogService = tourLogService ?? throw new ArgumentNullException(nameof(tourLogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            EventAggregator.Subscribe<SelectedTourChangedEvent>(OnSelectedTourChanged);
        }


        /// <summary>
        /// Opens a dialog to create a new tour log for the selected tour and initializes it with the name specified in NewLogName
        /// </summary>
        /// <param name="parameter"></param>
        private void AddNewTourLog(object? parameter)
        {
            _wpfService.SpawnEditTourLogWindow(SelectedTour!, new TourLog() { Comment = NewLogName! });
            NewLogName = string.Empty;
        }


        /// <summary>
        /// Deletes the currently selected tour log from the database and removes it from the local tour's logs collection
        /// </summary>
        /// <param name="parameter"></param>
        private async Task DeleteTourLog(object? parameter)
        {
            var success = await _tourLogService.DeleteTourLogAsync(SelectedLog!.LogId);

            if (success)
            {
                _logger.Info($"Deleted log with ID {SelectedLog.LogId}: {SelectedLog.Comment} from Tour with ID {SelectedTour!.TourId}: {SelectedTour.TourName}");

                // Remove the log from the local tour
                SelectedTour.Logs.Remove(SelectedLog);
            }
            else
            {
                _logger.Error($"Failed to delete log with ID {SelectedLog.LogId}: {SelectedLog.Comment} from Tour with ID {SelectedTour!.TourId}: {SelectedTour.TourName}");
            }
            SelectedLog = null;
        }


        /// <summary>
        /// Opens a dialog to edit the currently selected tour log for the selected tour
        /// </summary>
        /// <param name="parameter"></param>
        private void EditTourLog(object? parameter)
        {
            _wpfService.SpawnEditTourLogWindow(SelectedTour!, SelectedLog!);
        }


        /// <summary>
        /// Handles the event when the selected tour changes and updates the ViewModels own SelectedTour property
        /// </summary>
        /// <param name="e">The event containing the newly selected tour</param>
        private void OnSelectedTourChanged(SelectedTourChangedEvent e)
        {
            SelectedTour = e.SelectedTour;
        }
    }
}