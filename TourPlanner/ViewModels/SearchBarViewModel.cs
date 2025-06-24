using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model.Events;

namespace TourPlanner.ViewModels
{
    public class SearchBarViewModel : BaseViewModel
    {
        private readonly ILogger<SearchBarViewModel> _logger;
        
        // Commands
        private RelayCommand? _executeClearSearchQuery;
        public ICommand ExecuteClearSearchQuery => _executeClearSearchQuery ??= 
            new RelayCommand(ClearSearchQuery, _ => !string.IsNullOrEmpty(SearchQuery));
        
        // Stores the current search query
        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                RaisePropertyChanged(nameof(SearchQuery));
                
                // Inform others about the changed search query
                EventAggregator.Publish(new SearchQueryChangedEvent(_searchQuery));
            }
        }
        
        
        /// <summary>
        /// Initializes a new instance of the SearchBarViewModel class.
        /// </summary>
        /// <param name="eventAggregator">The event aggregator used for publishing and subscribing to events</param>
        public SearchBarViewModel(IEventAggregator eventAggregator, ILogger<SearchBarViewModel> logger) : base(eventAggregator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        
        /// <summary>
        /// Clears the search query and notifies other components about the change
        /// </summary>
        /// <param name="parameter">The parameter is not used, but required by the ICommand interface</param>
        private void ClearSearchQuery(object? parameter)
        {
            SearchQuery = string.Empty; // Clear the search query
            EventAggregator.Publish(new SearchQueryChangedEvent(_searchQuery)); // Inform others about the cleared search query
            _logger.Info("Search query cleared.");
        }
    }
}
