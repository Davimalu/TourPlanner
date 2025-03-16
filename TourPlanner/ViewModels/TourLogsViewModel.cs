using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.Models;
using TourPlanner.Views;

namespace TourPlanner.ViewModels
{
    public class TourLogsViewModel : BaseViewModel
    {
        private string _newLogName;
        public string NewLogName
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


        public TourLogsViewModel(TourListViewModel tourListViewModel)
        {
            tourListViewModel.SelectedTourChanged += (selectedTour) => SelectedTour = selectedTour; // Get the selected tour from the TourListViewModel
        }


        public ICommand ExecuteAddNewTourLog => new RelayCommand(_ =>
        {
            SelectedTour!.Logs.Add(new TourLog
            {
                Comment = NewLogName,
            });
            NewLogName = string.Empty;
        }, _ => SelectedTour != null && !string.IsNullOrEmpty(NewLogName));


        public ICommand ExecuteDeleteTourLog => new RelayCommand(_ =>
        {
            SelectedTour!.Logs.Remove(SelectedLog!);
            SelectedLog = null;
        }, _ => SelectedTour != null && SelectedLog != null);


        public ICommand ExecuteEditTourLog => new RelayCommand(_ =>
        {
            var editWindow = new EditTourLogWindow
            {
                DataContext = new EditTourLogViewModel(SelectedLog!)
            };

            editWindow.ShowDialog();

        }, _ => SelectedTour != null && SelectedLog != null);
    }
}