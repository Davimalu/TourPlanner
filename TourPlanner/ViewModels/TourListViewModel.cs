using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.DAL.ServiceAgents;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
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
        private readonly ITourService _tourService = new TourService();
        private readonly ILoggerWrapper _logger;


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
            _logger = LoggerFactory.GetLogger<TourListViewModel>();

            // Get a list of all tours from the REST API when the ViewModel is created
            LoadToursAsync();
        }


        public ICommand ExecuteAddNewTour => new RelayCommand(_ =>
        {
            _windowService.SpawnEditTourWindow(new Tour() { TourName = NewTourName!, TourId = -1 }); // ID -1 (i.e. an invalid ID) indicates that the Tour is new and not yet saved in the database
            NewTourName = string.Empty;

            // Refresh the list of tours
            LoadToursAsync();
        }, _ => !string.IsNullOrEmpty(NewTourName));


        public ICommand ExecuteDeleteTour => new RelayCommandAsync(async _ =>
        {
            var success = await _tourService.DeleteTourAsync(SelectedTour!.TourId); // Delete the tour via the REST API

            if (success)
            {
                _logger.Info($"Deleted tour with ID {SelectedTour.TourId}: {SelectedTour.TourName}");
                Tours?.Remove(SelectedTour); // Remove the tour from the local collection
            }
            else
            {
                _logger.Error($"Failed to delete tour with ID {SelectedTour.TourId}: {SelectedTour.TourName}");
                return;
            }
            
            SelectedTour = null;
        }, _ => SelectedTour != null);


        public ICommand ExecuteEditTour => new RelayCommand(_ =>
        {
            _windowService.SpawnEditTourWindow(SelectedTour!);

            // Refresh the list of tours
            LoadToursAsync();
        }, _ => SelectedTour != null);


        /// <summary>
        /// Helper method to load all tours from the REST API because the constructor cannot be async
        /// </summary>
        private async void LoadToursAsync()
        {
            try
            {
                var tourList = await _tourService.GetToursAsync();
                Tours = new ObservableCollection<Tour>(tourList ?? new List<Tour>());
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading tours: {ex.Message}");
            }
        }
    }
}
