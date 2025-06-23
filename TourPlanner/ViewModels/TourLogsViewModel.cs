using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Events;

namespace TourPlanner.ViewModels
{
    public class TourLogsViewModel : BaseViewModel
    {
        private readonly IUiService _uiService;
        private readonly ITourLogService _tourLogService;
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


        public TourLogsViewModel(IUiService uiService, ITourLogService tourLogService, IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _uiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
            _tourLogService = tourLogService ?? throw new ArgumentNullException(nameof(tourLogService));
            
            _logger = LoggerFactory.GetLogger<TourListViewModel>();
            
            EventAggregator.Subscribe<SelectedTourChangedEvent>(OnSelectedTourChanged);
        }


        public ICommand ExecuteAddNewTourLog => new RelayCommand(_ =>
        {
            _uiService.SpawnEditTourLogWindow(SelectedTour!, new TourLog() { Comment = NewLogName! });
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
            _uiService.SpawnEditTourLogWindow(SelectedTour!, SelectedLog!);
        }, _ => SelectedTour != null && SelectedLog != null);


        /// <summary>
        /// Handles the event when the selected tour changes and updates the ViewModels own SelectedTour property
        /// </summary>
        /// <param name="e">The event containing the newly selected tour</param>
        private async void OnSelectedTourChanged(SelectedTourChangedEvent e)
        {
            SelectedTour = e.SelectedTour;
        }
    }
}