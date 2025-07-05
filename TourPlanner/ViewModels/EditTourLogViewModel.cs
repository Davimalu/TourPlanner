using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Enums;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Events;

namespace TourPlanner.ViewModels
{
    public class EditTourLogViewModel : BaseViewModel
    {
        private readonly ITourService _tourService;
        private readonly ITourLogService _tourLogService;
        private readonly IAttributeService _attributeService;
        private readonly ILogger<EditTourLogViewModel> _logger;
        
        /* stores a copy of the original TourLog that will be edited by the user while this window is open
         * if the user cancels, the original TourLog remains unchanged | if the user saves, the original TourLog is overwritten with this one */
        private TourLog _editableTourLog = null!;
        public TourLog EditableTourLog
        {
            get => _editableTourLog;
            set
            {
                _editableTourLog = value;
                RaisePropertyChanged(nameof(EditableTourLog));
            }
        }
        
        
        // If the UI is bound to EditableTourLog.Comment directly, the setter of EditableTourLog isn't executed and the SaveButton isn't informed that its CanExecute state may have changed
        public string Comment
        {
            get => EditableTourLog.Comment;
            set
            {
                EditableTourLog.Comment = value;
                RaisePropertyChanged(nameof(EditableTourLog));
                
                // Notify the command that the state may have changed
                _executeSave?.RaiseCanExecuteChanged();
            }
        }


        private Tour _selectedTour;
        public Tour SelectedTour
        {
            get => _selectedTour;
            set
            {
                _selectedTour = value;
                RaisePropertyChanged(nameof(SelectedTour));
            }
        }

        
        // WPF can't bind to enums directly, so we use lists
        public List<Difficulty> Difficulties { get; set; }
        public List<Rating> Ratings { get; set; }
        
        
        // Commands
        private RelayCommandAsync? _executeSave;
        private RelayCommand? _executeCancel;
        
        public ICommand ExecuteSave => _executeSave ??= 
            new RelayCommandAsync(SaveSelectedTourLog, _ => !string.IsNullOrWhiteSpace(EditableTourLog.Comment));
        
        public ICommand ExecuteCancel => _executeCancel ??= 
            new RelayCommand(CancelEditTourLog, _ => true);


        /// <summary>
        /// Initializes a new instance of the <see cref="EditTourLogViewModel"/> class.
        /// </summary>
        /// <param name="selectedTour">The tour to which the log belongs</param>
        /// <param name="selectedTourLog">The tour log to edit or create</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the required services are null</exception>
        public EditTourLogViewModel(Tour selectedTour, TourLog selectedTourLog, ITourService tourService, ITourLogService tourLogService,
            IAttributeService attributeService, IEventAggregator eventAggregator, ILogger<EditTourLogViewModel> logger) : base(eventAggregator)
        {
            _tourService = tourService ?? throw new ArgumentNullException(nameof(tourService));
            _tourLogService = tourLogService ?? throw new ArgumentNullException(nameof(tourLogService));
            _attributeService = attributeService ?? throw new ArgumentNullException(nameof(attributeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _selectedTour = selectedTour;
            EditableTourLog = new TourLog(selectedTourLog); // Create a copy of the TourLog to edit (so that if the user cancels, the original TourLog remains unchanged)

            // Initialize enums
            Difficulties = new List<Difficulty> { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard };
            Ratings = new List<Rating> { Rating.Bad, Rating.Okay, Rating.Good, Rating.Great, Rating.Amazing };
        }


        /// <summary>
        /// Creates / updates the TourLog in the database and in the local Tour object
        /// </summary>
        /// <param name="parameter"></param>
        private async Task SaveSelectedTourLog(object? parameter)
        {
            // Check if the TourLog already exists in the SelectedTour (i.e. are we updating an existing TourLog or creating a new one?)
            TourLog? existingTourLog = _selectedTour.Logs.FirstOrDefault(log => log.LogId == EditableTourLog.LogId);
            
            // TourLog already exists -> update it
            if (existingTourLog != null)
            {
                _logger.Debug($"Updating TourLog with ID {EditableTourLog.LogId}: {EditableTourLog.Comment} from Tour with ID {SelectedTour.TourId}: {SelectedTour.TourName}");
                TourLog? updatedTourLog = await _tourLogService.UpdateTourLogAsync(EditableTourLog);

                if (updatedTourLog != null)
                {
                    // Update the local TourLog in the SelectedTour
                    int index = _selectedTour.Logs.IndexOf(existingTourLog);
                    if (index >= 0)
                    {
                        _selectedTour.Logs[index] = updatedTourLog;
                    }
                }
                else
                {
                    _logger.Error($"Failed to update TourLog with ID {EditableTourLog.LogId}: {EditableTourLog.Comment} from Tour with ID {SelectedTour.TourId}: {SelectedTour.TourName}");
                    return;
                }
            }
            // TourLog doesn't exist -> create it
            else
            {
                _logger.Debug($"Creating new TourLog for Tour with ID {SelectedTour.TourId}: {SelectedTour.TourName} with comment: {EditableTourLog.Comment}");
                TourLog? newTourLog = await _tourLogService.CreateTourLogAsync(SelectedTour.TourId, EditableTourLog);

                if (newTourLog != null)
                {
                    // Add the new TourLog to the SelectedTour
                    _selectedTour.Logs.Add(newTourLog);
                }
                else
                {
                    _logger.Error($"Failed to create TourLog with ID {EditableTourLog.LogId}: {EditableTourLog.Comment} from Tour with ID {SelectedTour.TourId}: {SelectedTour.TourName}");
                    return;
                }
            }
            _logger.Info($"TourLog with ID {EditableTourLog.LogId}: {EditableTourLog.Comment} saved successfully for Tour with ID {SelectedTour.TourId}: {SelectedTour.TourName}");
            
            // Calculate the attributes of the Tour (change whenever a log is added or updated) and update the Tour in the database
            _logger.Debug($"Recalculating attributes for Tour with ID {SelectedTour.TourId}: {SelectedTour.TourName}...");
            
            SelectedTour.Popularity = await _attributeService.CalculatePopularityAsync(SelectedTour);
            SelectedTour.ChildFriendlyRating = _attributeService.CalculateChildFriendliness(SelectedTour);
            await _tourService.UpdateTourAsync(SelectedTour);
            
            _logger.Debug($"Attributes for Tour with ID {SelectedTour.TourId}: {SelectedTour.TourName} calculated and saved successfully");
            
            // Close the window
            CloseWindow();
        }


        /// <summary>
        /// Cancels the edit operation and closes the window without saving changes
        /// </summary>
        /// <param name="parameter"></param>
        private void CancelEditTourLog(object? parameter)
        {
            // Close the window, discarding changes
            CloseWindow();
        }


        /// <summary>
        /// Closes the current window that this ViewModel is bound to
        /// </summary>
        private void CloseWindow()
        {
            // Request the UI to close the window
            EventAggregator.Publish(new CloseWindowRequestedEvent(this));
        }
    }
}