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
        private readonly IMapService _mapService;
        
        public WindowService(ITourLogService tourLogService, ITourService tourService, IMapService mapService)
        {
            _tourLogService = tourLogService ?? throw new ArgumentNullException(nameof(tourLogService));
            _tourService = tourService ?? throw new ArgumentNullException(nameof(tourService));
            _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
        }
        
        
        public void SpawnEditTourWindow(Tour selectedTour)
        {
            var editWindow = new EditTourWindow()
            {
                DataContext = new EditTourViewModel(selectedTour, _tourService, _mapService)
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
