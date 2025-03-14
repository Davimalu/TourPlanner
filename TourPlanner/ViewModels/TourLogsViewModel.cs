using System.Collections.ObjectModel;
using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.Enums;
using TourPlanner.Models;
using TourPlanner.Views;

namespace TourPlanner.ViewModels
{
    public class TourLogsViewModel : BaseViewModel
    {
        private ObservableCollection<TourLog> _tourLogs;
        public ObservableCollection<TourLog> TourLogs
        {
            get { return _tourLogs; }
            set
            {
                _tourLogs = value;
                RaisePropertyChanged(nameof(TourLogs));
            }
        }

        private string _newComment;
        public string NewComment
        {
            get { return _newComment; }
            set
            {
                _newComment = value;
                RaisePropertyChanged(nameof(NewComment));
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
                if (SelectedLog != null)
                {
                    // Fülle die Eingabefelder mit den Daten des ausgewählten Logs
                    NewComment = SelectedLog.Comment;
                }
            }
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get { return _isEditing; }
            set
            {
                _isEditing = value;
                RaisePropertyChanged(nameof(IsEditing));
            }
        }

        public TourLogsViewModel()
        {
            // Initialize dummy logs
            TourLogs = new ObservableCollection<TourLog>
            {
                new TourLog
                {
                    TimeStamp = DateTime.Now,
                    Comment = "Erster Log",
                    Difficulty = Difficulty.Easy,
                    DistanceTraveled = 10,
                    TimeTaken = DateTime.Now,
                    Rating = Rating.Good
                },
                new TourLog
                {
                    TimeStamp = DateTime.Now,
                    Comment = "Zweiter Log",
                    Difficulty = Difficulty.Medium,
                    DistanceTraveled = 20,
                    TimeTaken = DateTime.Now,
                    Rating = Rating.Great
                }
            };
        }

        public ICommand ExecuteAddNewTourLog => new RelayCommand(_ =>
        {
            TourLogs.Add(new TourLog
            {
                TimeStamp = DateTime.Now,
                Comment = NewComment,
                Difficulty = Difficulty.Easy,
                DistanceTraveled = 0,
                TimeTaken = DateTime.Now,
                Rating = Rating.Okay
            });
            NewComment = string.Empty;
        }, _ => !string.IsNullOrEmpty(NewComment));

        public ICommand ExecuteDeleteTourLog => new RelayCommand(_ =>
        {
            if (SelectedLog != null)
            {
                TourLogs.Remove(SelectedLog);
            }
            SelectedLog = null;
        }, _ => SelectedLog != null);

        public ICommand ExecuteEditTourLog => new RelayCommand(_ =>
        {
            if (SelectedLog != null)
            {
                var editWindow = new EditTourLogWindow
                {
                    DataContext = new EditTourLogViewModel { TourLog = SelectedLog }
                };
                editWindow.ShowDialog();
            }
        }, _ => SelectedLog != null);

        public ICommand ExecuteSaveTourLog => new RelayCommand(_ =>
        {
            if (SelectedLog != null)
            {
                SelectedLog.Comment = NewComment;
                IsEditing = false;
                NewComment = string.Empty;
            }
        }, _ => !string.IsNullOrEmpty(NewComment));

    }
}