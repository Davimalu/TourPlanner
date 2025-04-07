using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.Logic;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Models;
using TourPlanner.Views;

namespace TourPlanner.ViewModels
{
    public class TourLogsViewModel : BaseViewModel
    {
        private readonly ISelectedTourService _selectedTourService;
        private readonly IWindowService _windowService = WindowService.Instance;

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


        public TourLogsViewModel(ISelectedTourService selectedTourService)
        {
            _selectedTourService = selectedTourService;
            _selectedTourService.SelectedTourChanged += (selectedTour) => SelectedTour = selectedTour; // Get the selected tour from the service
        }


        public ICommand ExecuteAddNewTourLog => new RelayCommand(_ =>
        {
            SelectedTour!.Logs.Add(new TourLog
            {
                Comment = NewLogName!,
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
            _windowService.SpawnEditTourLogWindow(SelectedLog!);
        }, _ => SelectedTour != null && SelectedLog != null);
    }
}