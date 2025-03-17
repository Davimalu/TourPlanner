using System.Windows;
using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.Enums;
using TourPlanner.Models;

namespace TourPlanner.ViewModels
{
    public class EditTourLogViewModel : BaseViewModel
    {
        // The original TourLog before it was edited
        private readonly TourLog _originalTourLog;


        // The copy that will be edited
        private TourLog _editableTourLog;
        public TourLog EditableTourLog
        {
            get { return _editableTourLog; }
            set
            {
                _editableTourLog = value;
                RaisePropertyChanged(nameof(EditableTourLog));
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


        public EditTourLogViewModel(TourLog selectedTourLog)
        {
            _originalTourLog = selectedTourLog; // Store the original TourLog
            EditableTourLog = new TourLog() // Create a copy of the TourLog to edit
            {
                TimeStamp = selectedTourLog.TimeStamp,
                Comment = selectedTourLog.Comment,
                Difficulty = selectedTourLog.Difficulty,
                DistanceTraveled = selectedTourLog.DistanceTraveled,
                TimeTaken = selectedTourLog.TimeTaken,
                Rating = selectedTourLog.Rating
            }; 

            // Initialize enums
            Difficulties = new List<Difficulty> { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard };
            Ratings = new List<Rating> { Rating.Bad, Rating.Okay, Rating.Good, Rating.Great, Rating.Amazing };
        }


        public ICommand ExecuteSave => new RelayCommand(_ =>
        {
            // Copy the changes from the editable copy to the original
            _originalTourLog.TimeStamp = EditableTourLog.TimeStamp;
            _originalTourLog.Comment = EditableTourLog.Comment;
            _originalTourLog.Difficulty = EditableTourLog.Difficulty;
            _originalTourLog.DistanceTraveled = EditableTourLog.DistanceTraveled;
            _originalTourLog.TimeTaken = EditableTourLog.TimeTaken;
            _originalTourLog.Rating = EditableTourLog.Rating;

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