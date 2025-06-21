namespace TourPlanner.Logic.Interfaces;

public interface ISearchService
{
    string CurrentQuery { get; set; }
    event EventHandler<string>? QueryChanged;
}