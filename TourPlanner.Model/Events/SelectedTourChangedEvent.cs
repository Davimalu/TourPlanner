namespace TourPlanner.Model.Events;

public class SelectedTourChangedEvent
{
    public Tour? SelectedTour { get; set; }
    
    public SelectedTourChangedEvent(Tour? selectedTour)
    {
        SelectedTour = selectedTour; // null is allowed, e.g. when the user clears the selection in the UI
    }
}