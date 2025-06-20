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
        private readonly IOrsService _osrService;
        private readonly MapViewModel _mapViewModel;
        
        public WindowService(ITourLogService tourLogService, ITourService tourService, IOrsService iosrService, MapViewModel mapViewModel)
        {
            _tourLogService = tourLogService ?? throw new ArgumentNullException(nameof(tourLogService));
            _tourService = tourService ?? throw new ArgumentNullException(nameof(tourService));
            _osrService = iosrService ?? throw new ArgumentNullException(nameof(iosrService));
            _mapViewModel = mapViewModel ?? throw new ArgumentNullException(nameof(mapViewModel));
        }
        
        
        public void SpawnEditTourWindow(Tour selectedTour)
        {
            var editWindow = new EditTourWindow()
            {
                DataContext = new EditTourViewModel(_mapViewModel, selectedTour, _tourService, _osrService)
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
