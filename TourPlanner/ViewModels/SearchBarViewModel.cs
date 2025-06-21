using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;

namespace TourPlanner.ViewModels
{
    public class SearchBarViewModel : BaseViewModel
    {
        private readonly ISearchService _searchService;
        private readonly ILoggerWrapper _logger;
        
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                RaisePropertyChanged(nameof(SearchText));
                
                _searchService.CurrentQuery = _searchText;
            }
        }
        
        
        public SearchBarViewModel(ISearchService searchService)
        {
            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
            _logger = LoggerFactory.GetLogger<SearchBarViewModel>();
        }

        
        public ICommand ExecuteClearSearchQuery => new RelayCommand(_ =>
        {
            SearchText = string.Empty;
            _searchService.CurrentQuery = string.Empty;
            _logger.Info("Search query cleared.");
        }, _ => !string.IsNullOrEmpty(_searchText));
    }
}
