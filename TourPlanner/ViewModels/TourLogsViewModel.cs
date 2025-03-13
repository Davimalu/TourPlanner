using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.Enums;
using TourPlanner.Models;

namespace TourPlanner.ViewModels
{
    public class TourLogsViewModel : BaseViewModel
    {
        private TourLog _selectedLog;
        private string _newComment;
        private Difficulty _newDifficulty;
        private float _newDistanceTraveled;
        private DateTime _newTimeTaken;
        private Rating _newRating;

        public ObservableCollection<TourLog> TourLogs { get; set; }

        public TourLog SelectedLog
        {
            get => _selectedLog;
            set
            {
                _selectedLog = value;
                OnPropertyChanged();
            }
        }

        public string NewComment
        {
            get => _newComment;
            set
            {
                _newComment = value;
                OnPropertyChanged();
            }
        }

        public Difficulty NewDifficulty
        {
            get => _newDifficulty;
            set
            {
                _newDifficulty = value;
                OnPropertyChanged();
            }
        }

        public float NewDistanceTraveled
        {
            get => _newDistanceTraveled;
            set
            {
                _newDistanceTraveled = value;
                OnPropertyChanged();
            }
        }

        public DateTime NewTimeTaken
        {
            get => _newTimeTaken;
            set
            {
                _newTimeTaken = value;
                OnPropertyChanged();
            }
        }

        public Rating NewRating
        {
            get => _newRating;
            set
            {
                _newRating = value;
                OnPropertyChanged();
            }
        }

        public ICommand ExecuteAddNewTourLog { get; }
        public ICommand ExecuteDeleteTourLog { get; }

        public TourLogsViewModel()
        {
            TourLogs = new ObservableCollection<TourLog>();
            ExecuteAddNewTourLog = new RelayCommand(AddNewTourLog, CanAddNewTourLog);
            ExecuteDeleteTourLog = new RelayCommand(DeleteTourLog, CanDeleteTourLog);
        }

        private void AddNewTourLog(object parameter)
        {
            var newLog = new TourLog
            {
                TimeStamp = DateTime.Now,
                Comment = NewComment,
                Difficulty = NewDifficulty,
                DistanceTraveled = NewDistanceTraveled,
                TimeTaken = NewTimeTaken,
                Rating = NewRating
            };

            TourLogs.Add(newLog);
            ClearInputFields();
        }

        private bool CanAddNewTourLog(object parameter)
        {
            return !string.IsNullOrWhiteSpace(NewComment) && NewDistanceTraveled > 0 && NewTimeTaken != default;
        }

        private void DeleteTourLog(object parameter)
        {
            if (SelectedLog != null)
            {
                TourLogs.Remove(SelectedLog);
            }
        }

        private bool CanDeleteTourLog(object parameter)
        {
            return SelectedLog != null;
        }

        private void ClearInputFields()
        {
            NewComment = string.Empty;
            NewDifficulty = Difficulty.Easy;
            NewDistanceTraveled = 0;
            NewTimeTaken = default;
            NewRating = Rating.OneStar;
        }
    }
}