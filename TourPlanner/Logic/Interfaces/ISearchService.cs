namespace TourPlanner.Logic.Interfaces;

public interface ISearchService
{
    string CurrentQuery { get; set; }
    EventHandler<string> QueryChanged { get; set; }
}