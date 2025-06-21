using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;

namespace TourPlanner.Logic;

public class SearchService : ISearchService
{   
    private string _currentQuery = string.Empty;
    private readonly ILoggerWrapper _logger;

    public string CurrentQuery
    {
        get => _currentQuery;
        set
        {
            if (_currentQuery != value)
            {
                _currentQuery = value;
                QueryChanged?.Invoke(this, _currentQuery);
                
                _logger.Debug($"Search query updated: {_currentQuery}");
            }
        }
    }

    public event EventHandler<string>? QueryChanged;
    
    
    public SearchService()
    {
        _logger = LoggerFactory.GetLogger<SearchService>();
    }
}