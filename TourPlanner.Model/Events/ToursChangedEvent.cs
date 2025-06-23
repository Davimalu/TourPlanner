namespace TourPlanner.Model.Events;

/// <summary>
/// Event that is triggered when the list of tours changes
/// </summary>
public class ToursChangedEvent
{
    public List<Tour> Tours { get; set; }
    
    public ToursChangedEvent(List<Tour> tours)
    {
        Tours = tours;
    }
}