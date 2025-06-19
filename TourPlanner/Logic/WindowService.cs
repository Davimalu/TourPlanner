using TourPlanner.DAL.Interfaces;
using TourPlanner.DAL.ServiceAgents;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.ViewModels;
using TourPlanner.Views;

namespace TourPlanner.Logic
{
    internal class WindowService : IWindowService
    {
        private readonly ITourLogService _tourLogService;
        private readonly ITourService _tourService;
        private readonly IOSRService _iosrService;
        
        public WindowService(ITourLogService tourLogService, ITourService tourService, IOSRService iosrService)
        {
            _tourLogService = tourLogService ?? throw new ArgumentNullException(nameof(tourLogService));
            _tourService = tourService ?? throw new ArgumentNullException(nameof(tourService));
            _iosrService = iosrService ?? throw new ArgumentNullException(nameof(iosrService));
        }
        
        
        public void SpawnEditTourWindow(Tour selectedTour)
        {
            var editWindow = new EditTourWindow()
            {
                DataContext = new EditTourViewModel(selectedTour, _tourService, _iosrService)
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
