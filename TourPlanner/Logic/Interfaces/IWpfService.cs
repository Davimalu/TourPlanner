using TourPlanner.Model;
using TourPlanner.Model.Enums.MessageBoxAbstraction;

namespace TourPlanner.Logic.Interfaces
{
    public interface IWpfService
    {
        public void SpawnEditTourWindow(Tour selectedTour);
        public void SpawnEditTourLogWindow (Tour selectedTour, TourLog selectedTourLog);
        public MessageBoxResult ShowMessageBox(string title, string message, MessageBoxButton buttons, MessageBoxImage icon);
        public void ApplyLightTheme();
        public void ApplyDarkTheme();
        public void ExitApplication();
    }
}
