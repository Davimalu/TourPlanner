namespace TourPlanner.Model.Events;

public class SearchQueryChangedEvent
{
    public string SearchQuery { get; set; }
    
    public SearchQueryChangedEvent(string searchQuery)
    {
        SearchQuery = searchQuery ?? throw new ArgumentNullException(nameof(searchQuery));
    }
}