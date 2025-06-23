using System.ComponentModel;
using TourPlanner.Logic.Interfaces;

namespace TourPlanner.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        protected readonly IEventAggregator EventAggregator;
        
        protected BaseViewModel(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
