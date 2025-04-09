using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.DAL.ServiceAgents;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Models;
using TourPlanner.Views;

namespace TourPlanner.ViewModels
{
    public class TourLogsViewModel : BaseViewModel
    {
        private readonly ISelectedTourService _selectedTourService;
        private readonly IWindowService _windowService = WindowService.Instance;
        private readonly ITourLogService _tourLogService = new TourLogService();
        private readonly ILoggerWrapper _logger;

        private string? _newLogName;
        public string? NewLogName
        {
            get { return _newLogName; }
            set
            {
                _newLogName = value;
                RaisePropertyChanged(nameof(NewLogName));
            }
        }


        private TourLog? _selectedLog;
        public TourLog? SelectedLog
        {
            get { return _selectedLog; }
            set
            {
                _selectedLog = value;
                RaisePropertyChanged(nameof(SelectedLog));
            }
        }


        private Tour? _selectedTour;
        public Tour? SelectedTour
        {
            get { return _selectedTour; }
            set
            {
                _selectedTour = value;
                RaisePropertyChanged(nameof(SelectedTour));
            }
        }


        public TourLogsViewModel(ISelectedTourService selectedTourService)
        {
            _selectedTourService = selectedTourService;
            _selectedTourService.SelectedTourChanged += (selectedTour) => SelectedTour = selectedTour; // Get the selected tour from the service

            _logger = LoggerFactory.GetLogger<TourListViewModel>();
        }


        public ICommand ExecuteAddNewTourLog => new RelayCommand(_ =>
        {
            _windowService.SpawnEditTourLogWindow(SelectedTour!, new TourLog() { Comment = NewLogName! });
            NewLogName = string.Empty;
        }, _ => SelectedTour != null && !string.IsNullOrEmpty(NewLogName));


        public ICommand ExecuteDeleteTourLog => new RelayCommandAsync(async _ =>
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
        }, _ => SelectedTour != null && SelectedLog != null);


        public ICommand ExecuteEditTourLog => new RelayCommand(_ =>
        {
            _windowService.SpawnEditTourLogWindow(SelectedTour!, SelectedLog!);
        }, _ => SelectedTour != null && SelectedLog != null);
    }
}