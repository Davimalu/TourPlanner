using System.Windows;
using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.DAL.ServiceAgents;
using TourPlanner.Enums;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Models;

namespace TourPlanner.ViewModels
{
    public class EditTourLogViewModel : BaseViewModel
    {
        private readonly ITourService _tourService = new TourService();
        private readonly ILoggerWrapper _logger;

        // The original TourLog before it was edited
        private readonly TourLog _originalTourLog;


        // The copy that will be edited
        private TourLog _editableTourLog = null!;
        public TourLog EditableTourLog
        {
            get { return _editableTourLog; }
            set
            {
                _editableTourLog = value;
                RaisePropertyChanged(nameof(EditableTourLog));
            }
        }


        private Tour _selectedTour = null!;
        public Tour SelectedTour
        {
            get { return _selectedTour; }
            set
            {
                _selectedTour = value;
                RaisePropertyChanged(nameof(SelectedTour));
            }
        }


        public DateTime TimeStamp
        {
            get { return EditableTourLog.TimeStamp; }
            set
            {
                EditableTourLog.TimeStamp = value;
                RaisePropertyChanged(nameof(TimeStamp));
            }
        }


        public string Comment
        {
            get { return EditableTourLog.Comment; }
            set
            {
                EditableTourLog.Comment = value;
                RaisePropertyChanged(nameof(Comment));
            }
        }


        public Difficulty SelectedDifficulty
        {
            get { return EditableTourLog.Difficulty; } 
            set
            {
                EditableTourLog.Difficulty = value;
                RaisePropertyChanged(nameof(SelectedDifficulty));
            }
        }


        public float DistanceTraveled
        {
            get { return EditableTourLog.DistanceTraveled; }
            set
            {
                EditableTourLog.DistanceTraveled = value;
                RaisePropertyChanged(nameof(DistanceTraveled));
            }
        }

        public float TimeTaken
        {
            get { return EditableTourLog.TimeTaken; }
            set
            {
                EditableTourLog.TimeTaken = value;
                RaisePropertyChanged(nameof(TimeTaken));
            }
        }


        public Rating SelectedRating
        {
            get { return EditableTourLog.Rating; }
            set
            {
                EditableTourLog.Rating = value;
                RaisePropertyChanged(nameof(SelectedRating));
            }
        }


        public List<Difficulty> Difficulties { get; set; }
        public List<Rating> Ratings { get; set; }


        public EditTourLogViewModel(Tour selectedTour, TourLog selectedTourLog)
        {
            _logger = LoggerFactory.GetLogger<TourListViewModel>();

            _selectedTour = selectedTour;
            _originalTourLog = selectedTourLog; // Store the original TourLog
            EditableTourLog = new TourLog(_originalTourLog); // Create a copy of the TourLog to edit

            // Initialize enums
            Difficulties = new List<Difficulty> { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard };
            Ratings = new List<Rating> { Rating.Bad, Rating.Okay, Rating.Good, Rating.Great, Rating.Amazing };
        }


        public ICommand ExecuteSave => new RelayCommandAsync(async _ =>
        {
            // Copy the changes from the editable copy to the original
            _originalTourLog.TimeStamp = EditableTourLog.TimeStamp;
            _originalTourLog.Comment = EditableTourLog.Comment;
            _originalTourLog.Difficulty = EditableTourLog.Difficulty;
            _originalTourLog.DistanceTraveled = EditableTourLog.DistanceTraveled;
            _originalTourLog.TimeTaken = EditableTourLog.TimeTaken;
            _originalTourLog.Rating = EditableTourLog.Rating;

            // Check if the TourLog already exists in the SelectedTour (i.e. is this an update or a new entry?)
            TourLog? existingTourLog = _selectedTour.Logs.FirstOrDefault(log => log.LogId == _originalTourLog.LogId);

            // Add the TourLog to the Tour if it doesn't exist
            if (existingTourLog == null)
            {
                _selectedTour.Logs.Add(_originalTourLog);
            }

            // Save the changes via the API
            Tour? updatedTour = await _tourService.UpdateTourAsync(_selectedTour);

            if (updatedTour != null)
            {
                // Log the update
                _logger.Info($"TourLog {_originalTourLog.LogId} from Tour {updatedTour.TourId}: {updatedTour.TourName} updated successfully");
            }
            else
            {
                // Log the error
                _logger.Error($"Failed to update TourLog {_originalTourLog.LogId}");
            }

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