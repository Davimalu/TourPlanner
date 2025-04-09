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
        private readonly ITourLogService _tourLogService = new TourLogService();
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