using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TourPlanner.Commands;
using TourPlanner.DAL.ServiceAgents;
using TourPlanner.Enums;
using TourPlanner.Logic;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Models;
using TourPlanner.Views;

namespace TourPlanner.ViewModels
{
    public class TourListViewModel : BaseViewModel
    {
        private readonly ISelectedTourService _selectedTourService;
        private readonly IWindowService _windowService = WindowService.Instance;
        private readonly TourService _tourService = new TourService();


        private ObservableCollection<Tour>? _tours;
        public ObservableCollection<Tour>? Tours
        {
            get { return _tours; }
            set
            {
                _tours = value;
                RaisePropertyChanged(nameof(Tours));
            }
        }


        private string? _newTourName;
        public string? NewTourName
        {
            get { return _newTourName; }
            set
            {
                _newTourName = value;
                RaisePropertyChanged(nameof(NewTourName));
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
                _selectedTourService.SelectedTour = _selectedTour; // Update the selected tour in the service
            }
        }


        public TourListViewModel(ISelectedTourService selectedTourService)
        {
            _selectedTourService = selectedTourService;

            // Get a list of all tours from the REST API
            LoadToursAsync();
        }


        private async void LoadToursAsync()
        {
            try
            {
                // Await the async call to get the list of tours
                var tourList = await _tourService.GetToursAsync();
                Tours = new ObservableCollection<Tour>(tourList);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading tours: {ex.Message}");
            }
        }


        public ICommand ExecuteAddNewTour => new RelayCommand(_ =>
        {
            Tours?.Add(new Tour() { TourName = NewTourName! });
            NewTourName = string.Empty;
        }, _ => !string.IsNullOrEmpty(NewTourName));


        public ICommand ExecuteDeleteTour => new RelayCommand(_ =>
        {
            if (SelectedTour != null)
            {
                Tours?.Remove(SelectedTour);
            }
            SelectedTour = null;
        }, _ => SelectedTour != null);


        public ICommand ExecuteEditTour => new RelayCommand(_ =>
        {
            _windowService.SpawnEditTourWindow(SelectedTour!);
        }, _ => SelectedTour != null);
    }
}
