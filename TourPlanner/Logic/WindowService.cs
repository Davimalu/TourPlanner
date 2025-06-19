using TourPlanner.DAL.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.ViewModels;
using TourPlanner.Views;

namespace TourPlanner.Logic
{
    internal class WindowService : IWindowService
    {
        private readonly ITourLogService _tourLogService;
        
        public WindowService(ITourLogService tourLogService)
        {
            _tourLogService = tourLogService ?? throw new ArgumentNullException(nameof(tourLogService));
        }
        
        
        public void SpawnEditTourWindow(Tour selectedTour)
        {
            var editWindow = new EditTourWindow()
            {
                DataContext = new EditTourViewModel(selectedTour)
            };

            editWindow.ShowDialog();
        }


        public void SpawnEditTourLogWindow(Tour selectedTour, TourLog selectedTourLog)
        {
            var editWindow = new EditTourLogWindow
            {
                DataContext = new EditTourLogViewModel(selectedTour, selectedTourLog, _tourLogService)
            };

            editWindow.ShowDialog();
        }
    }
}
