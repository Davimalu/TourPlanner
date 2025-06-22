using TourPlanner.Logic.Interfaces;

namespace TourPlanner.Logic;

public class EventService : IEventService
{
    public event EventHandler? ToursChanged;
    
    public void RaiseToursChanged()
    {
        ToursChanged?.Invoke(this, EventArgs.Empty);
    }
}