using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;

namespace TourPlanner.ViewModels
{
    public class SearchBarViewModel : BaseViewModel
    {
        private readonly ISearchQueryService _searchQueryService;
        private readonly ILoggerWrapper _logger;
        
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                RaisePropertyChanged(nameof(SearchText));
                
                _searchQueryService.CurrentQuery = _searchText;
            }
        }
        
        
        public SearchBarViewModel(ISearchQueryService searchQueryService)
        {
            _searchQueryService = searchQueryService ?? throw new ArgumentNullException(nameof(searchQueryService));
            _logger = LoggerFactory.GetLogger<SearchBarViewModel>();
        }

        
        public ICommand ExecuteClearSearchQuery => new RelayCommand(_ =>
        {
            SearchText = string.Empty;
            _searchQueryService.CurrentQuery = string.Empty;
            _logger.Info("Search query cleared.");
        }, _ => !string.IsNullOrEmpty(_searchText));
    }
}
