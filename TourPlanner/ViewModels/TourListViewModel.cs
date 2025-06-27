using System.Collections.ObjectModel;
using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Events;

namespace TourPlanner.ViewModels
{
    public class TourListViewModel : BaseViewModel
    {
        private readonly IWpfService _wpfService;
        private readonly ITourService _tourService;
        private readonly ISearchService _searchService;
        private readonly IIoService _ioService;
        private readonly IPdfService _pdfService;
        private readonly ILogger<TourListViewModel> _logger;

        // Commands
        private RelayCommandAsync? _executeAddNewTour;
        private RelayCommandAsync? _executeEditTour;
        private RelayCommandAsync? _executeDeleteTour;
        private RelayCommandAsync? _executeExportTour;

        public ICommand ExecuteAddNewTour => _executeAddNewTour ??= 
            new RelayCommandAsync(async _ => await AddNewTour(), _ => !string.IsNullOrEmpty(NewTourName));
        
        public ICommand ExecuteEditTour => _executeEditTour ??=
            new RelayCommandAsync(async _ => await EditExistingTour(), _ => SelectedTour != null);
        
        public ICommand ExecuteDeleteTour => _executeDeleteTour ??=
            new RelayCommandAsync(async _ => await DeleteSelectedTour(), _ => SelectedTour != null);

        public ICommand ExecuteExportTour => _executeExportTour ??=
            new RelayCommandAsync(async _ => await ExportSelectedTourAsPdfAsync(), _ => SelectedTour != null);
        
        // UI Bindings
        
        // This flag indicates whether the user has applied a search filter
        private bool _filterActive = false;

        // We store all tours in _tours | When the user applies a search query, we filter the tours and store them in _filteredTours
        // Depending on the _filterActive flag, the Tours property returns either _tours or _filteredTours
        private ObservableCollection<Tour>? _filteredTours;
        private ObservableCollection<Tour>? _tours;

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
            get => _newTourName;
            set
            {
                _newTourName = value;
                RaisePropertyChanged(nameof(NewTourName));
                
                // Notify the AddNewTour command that its execution state may have changed
                _executeAddNewTour?.RaiseCanExecuteChanged();
            }
        }

        
        private Tour? _selectedTour;
        public Tour? SelectedTour
        {
            get => _selectedTour;
            set
            {
                _selectedTour = value;
                RaisePropertyChanged(nameof(SelectedTour));
                
                // Inform others that the user selected a new tour
                EventAggregator.Publish(new SelectedTourChangedEvent(_selectedTour));

                // Inform the commands that their execution state may have changed
                _executeDeleteTour?.RaiseCanExecuteChanged();
                _executeEditTour?.RaiseCanExecuteChanged();
            }
        }


        public TourListViewModel(ITourService tourService, IWpfService wpfService, ISearchService searchService,
            IIoService ioService, IPdfService pdfService, IEventAggregator eventAggregator, ILogger<TourListViewModel> logger) : base(eventAggregator)
        {
            _tourService = tourService ?? throw new ArgumentNullException(nameof(tourService));
            _wpfService = wpfService ?? throw new ArgumentNullException(nameof(wpfService));
            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
            _ioService = ioService ?? throw new ArgumentNullException(nameof(ioService));
            _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Subscribe to changes in the search query to filter tours
            EventAggregator.Subscribe<SearchQueryChangedEvent>(OnSearchQueryChanged);

            // Subscribe to changes in the tours to refresh the list of tours
            EventAggregator.Subscribe<ToursChangedEvent>(OnToursChangedAsync);

            // On Startup, load all tours from the REST API
            _ = LoadToursAsync();
        }


        /// <summary>
        /// Opens the dialog to add a new tour and initializes it with the provided name
        /// </summary>
        private async Task AddNewTour()
        {
            _wpfService.SpawnEditTourWindow(new Tour()
            {
                // ID -1 (i.e. an invalid ID) indicates that the Tour is new and not yet saved in the database
                TourName = NewTourName!, TourId = -1
            }); 
            NewTourName = string.Empty;
            
            // Inform other components that a new tour has been added
            EventAggregator.Publish(new ToursChangedEvent(_tours?.ToList() ?? new List<Tour>()));

            // Refresh the list of tours
            await LoadToursAsync();
        }


        /// <summary>
        /// Deletes the currently selected tour via the REST API and updates the local collection
        /// </summary>
        private async Task DeleteSelectedTour()
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

            // Inform other components that the tours have changed
            EventAggregator.Publish(new ToursChangedEvent(_tours?.ToList() ?? new List<Tour>()));
        }


        /// <summary>
        /// Opens the dialog to edit the currently selected tour
        /// </summary>
        private async Task EditExistingTour()
        {
            _wpfService.SpawnEditTourWindow(SelectedTour!);

            // Refresh the list of tours
            await LoadToursAsync();
        }
        
        
        /// <summary>
        /// Searches for tours (including their logs) based on the provided query.
        /// </summary>
        /// <param name="query">The search query to filter Tours and TourLogs by </param>
        private async Task HandleSearchQueryChangedAsync(SearchQueryChangedEvent query)
        {
            if (_tours == null || _tours.Count == 0)
            {
                _logger.Warn("No tours available to search.");
                return;
            }
 
            if (string.IsNullOrEmpty(query.SearchQuery))
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
                List<Tour> filteredTours = await _searchService.SearchToursAsync(query.SearchQuery, _tours.ToList());
                _filteredTours = new ObservableCollection<Tour>(filteredTours);
                _logger.Info($"Found {filteredTours.Count} tours matching the query: {query}");

                // Notify the UI that the Tours collection has changed
                RaisePropertyChanged(nameof(Tours));
            }
            catch (Exception ex)
            {
                _logger.Error($"Error searching tours: {ex.Message}");
            }

            // Inform other components that the tours have changed
            EventAggregator.Publish(new ToursChangedEvent(_filteredTours?.ToList() ?? new List<Tour>()));
        }
        

        /// <summary>
        /// Handles the SearchQueryChangedEvent to filter tours based on the search query
        /// </summary>
        /// <param name="query">The search query to filter Tours and TourLogs by </param>
        private async void OnSearchQueryChanged(SearchQueryChangedEvent query)
        {
            try
            {
                await HandleSearchQueryChangedAsync(query);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error handling search query change: {ex.Message}");
            }
        }


        /// <summary>
        /// Exports the selected tour as a PDF file
        /// </summary>
        /// <returns>True if the export was successful, false otherwise</returns>
        private Task<bool> ExportSelectedTourAsPdfAsync()
        {
            if (SelectedTour == null)
            {
                _logger.Warn("No tour selected for export.");
                return Task.FromResult(false);
            }

            // Get the file path from the user
            string filePath = _ioService.OpenFileSaveDialog(
                "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
                "Export Tour as PDF",
                $"{SelectedTour.TourName}.pdf");

            if (string.IsNullOrEmpty(filePath))
            {
                _logger.Warn("Export canceled by user.");
                return Task.FromResult(false);
            }

            _logger.Info($"Exporting tour {SelectedTour.TourName} (ID: {SelectedTour.TourId}) to PDF at {filePath}...");

            // Export the tour to PDF
            return _pdfService.ExportTourAsPdfAsync(SelectedTour, filePath);
        }
        
        
        
        /// <summary>
        /// Handles the ToursChangedEvent to reload tours when they change
        /// </summary>
        private async Task HandleToursChangedAsync()
        {
            _logger.Debug("Tours changed event received, reloading tours...");
            await LoadToursAsync();
        }


        /// <summary>
        /// Handles the ToursChangedEvent to reload tours when they change
        /// </summary>
        /// <param name="toursChangedEvent">The event containing the updated tours</param>
        private async void OnToursChangedAsync(ToursChangedEvent toursChangedEvent)
        {
            try
            {
                await HandleToursChangedAsync();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error handling tours changed event: {ex.Message}");
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


        /// <summary>
        /// Disposes of the ViewModel, unsubscribing from events to prevent memory leaks
        /// </summary>
        public void Dispose()
        {
            // Unsubscribe from events to prevent memory leaks
            EventAggregator.Unsubscribe<ToursChangedEvent>(OnToursChangedAsync);
            EventAggregator.Unsubscribe<SearchQueryChangedEvent>(OnSearchQueryChanged);
        }
    }
}