namespace TourPlanner.Logic.Interfaces;

public interface ISearchQueryService
{
    string CurrentQuery { get; set; }
    event EventHandler<string>? QueryChanged;
}