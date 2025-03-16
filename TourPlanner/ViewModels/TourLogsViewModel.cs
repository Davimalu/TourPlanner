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


        public TourLogsViewModel(TourListViewModel tourListViewModel)
        {
            tourListViewModel.SelectedTourChanged += (selectedTour) => SelectedTour = selectedTour; // Get the selected tour from the TourListViewModel
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
                    DataContext = new EditTourLogViewModel(SelectedLog, updatedLog =>
                    {
                        // Update the selected log with the changes
                        SelectedLog.Comment = updatedLog.Comment;
                        SelectedLog.Difficulty = updatedLog.Difficulty;
                        SelectedLog.DistanceTraveled = updatedLog.DistanceTraveled;
                        SelectedLog.TimeTaken = updatedLog.TimeTaken;
                        SelectedLog.Rating = updatedLog.Rating;

                        // Notify the UI that the logs have changed
                        RaisePropertyChanged(nameof(TourLogs));
                    })
                };

                editWindow.ShowDialog();
            }
        }, _ => SelectedLog != null);
    }
}