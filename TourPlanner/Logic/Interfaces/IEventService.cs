namespace TourPlanner.Logic.Interfaces;

public interface IEventService
{
    event EventHandler ToursChanged;
    void RaiseToursChanged();
}