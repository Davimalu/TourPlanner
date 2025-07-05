using TourPlanner.ViewModels;

namespace TourPlanner.Model.Events;

public class CloseWindowRequestedEvent
{
    // This event has to reside in the TourPlanner project since TourPlanner.Model has no reference to the BaseViewModel
    public BaseViewModel DataContextOfWindowToClose { get; }
    
    public CloseWindowRequestedEvent(BaseViewModel dataContextOfWindowToClose)
    {
        DataContextOfWindowToClose = dataContextOfWindowToClose;
    }
}