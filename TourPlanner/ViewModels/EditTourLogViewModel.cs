using System.Windows;
using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Enums;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;

namespace TourPlanner.ViewModels
{
    public class EditTourLogViewModel : BaseViewModel
    {
        private readonly ITourLogService _tourLogService;
        private readonly ITourService _tourService;
        private readonly IAttributeService _attributeService;
        private readonly ILoggerWrapper _logger;
        
        // Copy of the original TourLog to edit (to avoid changing the original UNTIL the user saves)
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


        public List<Difficulty> Difficulties { get; set; }
        public List<Rating> Ratings { get; set; }


        public EditTourLogViewModel(Tour selectedTour, ITourService tourService, TourLog selectedTourLog, ITourLogService tourLogService, IAttributeService attributeService)
        {
            _tourService = tourService ?? throw new ArgumentNullException(nameof(tourService));
            _tourLogService = tourLogService ?? throw new ArgumentNullException(nameof(tourLogService));
            _attributeService = attributeService ?? throw new ArgumentNullException(nameof(attributeService));
            _logger = LoggerFactory.GetLogger<TourListViewModel>();

            _selectedTour = selectedTour;
            EditableTourLog = new TourLog(selectedTourLog); // Create a copy of the TourLog to edit (so that if the user cancels, the original TourLog remains unchanged)

            // Initialize enums (WPF can't bind to enums directly, so we use lists)
            Difficulties = new List<Difficulty> { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard };
            Ratings = new List<Rating> { Rating.Bad, Rating.Okay, Rating.Good, Rating.Great, Rating.Amazing };
        }


        public ICommand ExecuteSave => new RelayCommandAsync(async _ =>
        {
            // Check if the TourLog already exists in the SelectedTour (i.e. are we updating an existing TourLog or creating a new one?)
            TourLog? existingTourLog = _selectedTour.Logs.FirstOrDefault(log => log.LogId == EditableTourLog.LogId);
            
            // TourLog already exists -> update it
            if (existingTourLog != null)
            {
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
                }
            }
            // TourLog doesn't exist -> create it
            else
            {
                TourLog? newTourLog = await _tourLogService.CreateTourLogAsync(SelectedTour.TourId, EditableTourLog);

                if (newTourLog != null)
                {
                    // Add the new TourLog to the SelectedTour
                    _selectedTour.Logs.Add(newTourLog);
                }
                else
                {
                    _logger.Error($"Failed to create TourLog with ID {EditableTourLog.LogId}: {EditableTourLog.Comment} from Tour with ID {SelectedTour.TourId}: {SelectedTour.TourName}");
                }
            }
            
            // Calculate the attributes of the Tour (change whenever a log is added or updated) and update the Tour in the database
            SelectedTour.Popularity = await _attributeService.CalculatePopularityAsync(SelectedTour);
            SelectedTour.ChildFriendlyRating = _attributeService.CalulateChildFriendliness(SelectedTour);
            await _tourService.UpdateTourAsync(SelectedTour);

            // Close the window
            CloseWindow();
        });


        public ICommand ExecuteCancel => new RelayCommand(_ =>
        {
            // Close the window, discarding changes
            CloseWindow();
        });


        private void CloseWindow()
        {
            // Close the window
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}