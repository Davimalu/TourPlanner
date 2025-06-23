using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Events;

namespace TourPlanner.ViewModels
{
    public class TourListViewModel : BaseViewModel
    {
        private readonly IWindowService _windowService;
        private readonly ITourService _tourService;
        private readonly ISearchService _searchService;
        private readonly IIoService _ioService;
        private readonly IPdfService _pdfService;
        private readonly ILoggerWrapper _logger;

        // Commands
        private RelayCommandAsync? _executeDeleteTour;
        private RelayCommandAsync? _executeExportTour;

        public ICommand ExecuteDeleteTour => _executeDeleteTour ??=
            new RelayCommandAsync(async _ => await DeleteSelectedTour(), _ => SelectedTour != null);

        public ICommand ExecuteExportTour => _executeExportTour ??=
            new RelayCommandAsync(async _ => await ExportSelectedTourAsPdfAsync(), _ => SelectedTour != null);

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
                
                // Inform others that the user selected a new tour
                EventAggregator.Publish(new SelectedTourChangedEvent(_selectedTour));

                // Inform the Delete Action that the SelectedTour has changed
                _executeDeleteTour?.RaiseCanExecuteChanged();
            }
        }


        public TourListViewModel(ITourService tourService, IWindowService windowService, ISearchService searchService,
            IIoService ioService, IPdfService pdfService, IEventAggregator eventAggregator) : base(eventAggregator)
        {
            _tourService = tourService ?? throw new ArgumentNullException(nameof(tourService));
            _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
            _ioService = ioService ?? throw new ArgumentNullException(nameof(ioService));
            _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
            _logger = LoggerFactory.GetLogger<TourListViewModel>();

            // Subscribe to changes in the search query to filter tours
            EventAggregator.Subscribe<SearchQueryChangedEvent>(OnSearchQueryChanged);

            // Subscribe to changes in the tours to refresh the list of tours
            EventAggregator.Subscribe<ToursChangedEvent>(OnToursChangedAsync);

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

            // Inform other components that the tours have changed
            EventAggregator.Publish(new ToursChangedEvent(_tours?.ToList() ?? new List<Tour>()));
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
        private async void OnSearchQueryChanged(SearchQueryChangedEvent query)
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
        /// <param name="toursChangedEvent">The event containing the updated tours</param>
        private async void OnToursChangedAsync(ToursChangedEvent toursChangedEvent)
        {
            _logger.Debug("Tours changed event received, reloading tours...");
            await LoadToursAsync();
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