using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;

namespace TourPlanner.ViewModels
{
    public class TourListViewModel : BaseViewModel
    {
        private readonly ISelectedTourService _selectedTourService;
        private readonly IWindowService _windowService;
        private readonly ITourService _tourService;
        private readonly ISearchQueryService _searchQueryService;
        private readonly ISearchService _searchService;
        private readonly IEventService _eventService;
        private readonly ILoggerWrapper _logger;
        
        // Commands
        private RelayCommandAsync? _executeDeleteTour;
        
        public ICommand ExecuteDeleteTour => _executeDeleteTour ??= 
            new RelayCommandAsync(async _ => await DeleteSelectedTour(), _ => SelectedTour != null);

        [Description(
            "when the user searches for a tour, this flag is set to true and the Tours collection is filtered accordingly")]
        private bool _filterActive = false;

        private ObservableCollection<Tour>? _filteredTours;
        private ObservableCollection<Tour>? _tours;

        [Description(
            "contains all tours that are currently loaded from the REST API | when _filterActive is true, this collection is filtered and the Tours property returns _filteredTours instead")]
        public ObservableCollection<Tour>? Tours
        {
            get
            {
                if (_filterActive && _filteredTours != null)
                {
                    return _filteredTours;
                }

                return _tours;
            }
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
                
                // Inform the Delete Action that the SelectedTour has changed
                _executeDeleteTour?.RaiseCanExecuteChanged();
            }
        }


        public TourListViewModel(ISelectedTourService selectedTourService, ITourService tourService,
            IWindowService windowService, ISearchQueryService searchQueryService, ISearchService searchService,
            IEventService eventService)
        {
            _selectedTourService = selectedTourService ?? throw new ArgumentNullException(nameof(selectedTourService));
            _tourService = tourService ?? throw new ArgumentNullException(nameof(tourService));
            _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
            _searchQueryService = searchQueryService ?? throw new ArgumentNullException(nameof(searchQueryService));
            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            _logger = LoggerFactory.GetLogger<TourListViewModel>();

            // Subscribe to changes in the search query to filter tours
            _searchQueryService.QueryChanged += async (sender, query) =>
            {
                await SearchToursAsync(query);
            };

            // Subscribe to changes in the tours to refresh the list of tours
            _eventService.ToursChanged += async (sender, tours) =>
            {
                await LoadToursAsync();
            };

            // On Startup, load all tours from the REST API
            _ = LoadToursAsync();
        }


        public ICommand ExecuteAddNewTour => new RelayCommand(_ =>
        {
            _windowService.SpawnEditTourWindow(new Tour()
            {
                TourName = NewTourName!, TourId = -1
            }); // ID -1 (i.e. an invalid ID) indicates that the Tour is new and not yet saved in the database
            NewTourName = string.Empty;

            // Refresh the list of tours
            LoadToursAsync();
        }, _ => !string.IsNullOrEmpty(NewTourName));

        
        public async Task DeleteSelectedTour()
        {
            if (SelectedTour == null)
            {
                _logger.Warn("No tour selected for deletion.");
                return;
            }

            var success = await _tourService.DeleteTourAsync(SelectedTour.TourId); // Delete the tour via the REST API

            if (success)
            {
                _logger.Info($"Deleted tour with ID {SelectedTour.TourId}: {SelectedTour.TourName}");
                Tours?.Remove(SelectedTour); // Remove the tour from the local collection too
            }
            else
            {
                _logger.Error($"Failed to delete tour with ID {SelectedTour.TourId}: {SelectedTour.TourName}");
            }

            SelectedTour = null;
        }


        public ICommand ExecuteEditTour => new RelayCommand(_ =>
        {
            _windowService.SpawnEditTourWindow(SelectedTour!);

            // Refresh the list of tours
            LoadToursAsync();
        }, _ => SelectedTour != null);


        /// <summary>
        /// Searches for tours (including their logs) based on the provided query.
        /// </summary>
        /// <param name="query">The search query to filter Tours and TourLogs by </param>
        private async Task SearchToursAsync(string query)
        {
            if (_tours == null || _tours.Count == 0)
            {
                _logger.Warn("No tours available to search.");
                return;
            }

            if (string.IsNullOrEmpty(query))
            {
                _logger.Info("Search query is empty, showing all tours.");
                _filterActive = false;

                // Notify the UI that the Tours collection has changed
                RaisePropertyChanged(nameof(Tours));

                return;
            }

            _filterActive = true;

            try
            {
                List<Tour> filteredTours = await _searchService.SearchToursAsync(query, _tours.ToList());
                _filteredTours = new ObservableCollection<Tour>(filteredTours);
                _logger.Info($"Found {filteredTours.Count} tours matching the query: {query}");

                // Notify the UI that the Tours collection has changed
                RaisePropertyChanged(nameof(Tours));
            }
            catch (Exception ex)
            {
                _logger.Error($"Error searching tours: {ex.Message}");
            }
        }


        /// <summary>
        /// Helper method to load all tours from the REST API because the constructor cannot be async
        /// </summary>
        private async Task LoadToursAsync()
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