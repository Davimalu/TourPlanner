using TourPlanner.Logic.Interfaces;

namespace TourPlanner.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public MainWindowViewModel(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }
    }
}
