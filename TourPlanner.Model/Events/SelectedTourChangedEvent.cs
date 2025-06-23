namespace TourPlanner.Model.Events;

public class SelectedTourChangedEvent
{
    public Tour? SelectedTour { get; set; }
    
    public SelectedTourChangedEvent(Tour? selectedTour)
    {
        SelectedTour = selectedTour ?? throw new ArgumentNullException(nameof(selectedTour));
    }
}