using System.Windows;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.ViewModels;
using TourPlanner.Views;

namespace TourPlanner.Logic
{
    /// <summary>
    /// Provides an abstraction layer for other classes that need to spawn windows or show message boxes
    /// </summary>
    public class WpfService : IWpfService
    {
        private readonly ITourLogService _tourLogService;
        private readonly ITourService _tourService;
        private readonly IOrsService _osrService;
        private readonly IMapService _mapService;
        private readonly IAttributeService _attributeService;
        private readonly IEventAggregator _eventAggregator;
        
        private readonly MapViewModel _mapViewModel;
        
        public WpfService(ITourLogService tourLogService, ITourService tourService, IOrsService orsService, IMapService mapService, MapViewModel mapViewModel, IAttributeService attributeService, IEventAggregator eventAggregator)
        {
            _tourLogService = tourLogService ?? throw new ArgumentNullException(nameof(tourLogService));
            _tourService = tourService ?? throw new ArgumentNullException(nameof(tourService));
            _osrService = orsService ?? throw new ArgumentNullException(nameof(orsService));
            _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
            _mapViewModel = mapViewModel ?? throw new ArgumentNullException(nameof(mapViewModel));
            _attributeService = attributeService ?? throw new ArgumentNullException(nameof(attributeService));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }
        
        
        /// <summary>
        /// Spawns the window for adding / editing a tour
        /// </summary>
        public void SpawnEditTourWindow(Tour selectedTour)
        {
            var editWindow = new EditTourWindow()
            {
                DataContext = new EditTourViewModel(selectedTour, _tourService, _osrService, _mapService, _eventAggregator),
                Map = { DataContext = _mapViewModel}
            };

            editWindow.ShowDialog();
        }


        /// <summary>
        /// Spawns the window for adding / editing a tour log
        /// </summary>
        public void SpawnEditTourLogWindow(Tour selectedTour, TourLog selectedTourLog)
        {
            var editWindow = new EditTourLogWindow
            {
                DataContext = new EditTourLogViewModel(selectedTour, _tourService, selectedTourLog, _tourLogService, _attributeService, _eventAggregator),
            };

            editWindow.ShowDialog();
        }

        
        /// <summary>
        /// Shows a message box with the specified parameters.
        /// </summary>
        /// <param name="title">The title of the message box</param>
        /// <param name="message">The message to display in the message box</param>
        /// <param name="buttons">The buttons to display in the message box</param>
        /// <param name="icon">The icon to display in the message box</param>
        /// <returns>The result of the message box interaction</returns>
        public Model.Enums.MessageBoxAbstraction.MessageBoxResult ShowMessageBox(string title, string message, Model.Enums.MessageBoxAbstraction.MessageBoxButton buttons, Model.Enums.MessageBoxAbstraction.MessageBoxImage icon)
        {
            var wpfUiButtons = MapAbstractionToWpfUiButton(buttons);
            var wpfUiIcon = MapAbstractionToWpfUiIcon(icon);

            return MapWpfUiResultToAbstraction(MessageBox.Show(message, title, wpfUiButtons, wpfUiIcon));
        }

        
        /// <summary>
        /// Exits the application gracefully
        /// </summary>
        public void ExitApplication()
        {
            Application.Current.Shutdown();
        }


        /// <summary>
        /// Maps the abstracted message box button enum to the WPF UI equivalent
        /// </summary>
        /// <param name="button">The abstracted message box button enum to map</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the provided button does not match any known values</exception>
        private MessageBoxButton MapAbstractionToWpfUiButton(Model.Enums.MessageBoxAbstraction.MessageBoxButton button)
        {
            return button switch
            {
                Model.Enums.MessageBoxAbstraction.MessageBoxButton.OK => MessageBoxButton.OK,
                Model.Enums.MessageBoxAbstraction.MessageBoxButton.OKCancel => MessageBoxButton.OKCancel,
                Model.Enums.MessageBoxAbstraction.MessageBoxButton.YesNo => MessageBoxButton.YesNo,
                Model.Enums.MessageBoxAbstraction.MessageBoxButton.YesNoCancel => MessageBoxButton.YesNoCancel,
                _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
            };
        }


        /// <summary>
        /// Maps the abstracted message box icon enum to the WPF UI equivalent
        /// </summary>
        /// <param name="icon">The abstracted message box icon enum to map</param>
        /// <returns>Returns the corresponding WPF UI MessageBoxImage</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the provided icon does not match any known values</exception>
        private MessageBoxImage MapAbstractionToWpfUiIcon(Model.Enums.MessageBoxAbstraction.MessageBoxImage icon)
        {
            return icon switch
            {
                Model.Enums.MessageBoxAbstraction.MessageBoxImage.None => MessageBoxImage.None,
                Model.Enums.MessageBoxAbstraction.MessageBoxImage.Information => MessageBoxImage.Information,
                Model.Enums.MessageBoxAbstraction.MessageBoxImage.Warning => MessageBoxImage.Warning,
                Model.Enums.MessageBoxAbstraction.MessageBoxImage.Error => MessageBoxImage.Error,
                Model.Enums.MessageBoxAbstraction.MessageBoxImage.Question => MessageBoxImage.Question,
                _ => throw new ArgumentOutOfRangeException(nameof(icon), icon, null)
            };
        }


        /// <summary>
        /// Maps the WPF UI MessageBoxResult to the abstracted MessageBoxResult enum
        /// </summary>
        /// <param name="result">The WPF UI MessageBoxResult to map</param>
        /// <returns>Returns the corresponding abstracted MessageBoxResult</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the provided result does not match any known values</exception>
        private Model.Enums.MessageBoxAbstraction.MessageBoxResult MapWpfUiResultToAbstraction(MessageBoxResult result)
        {
            return result switch
            {
                MessageBoxResult.OK => Model.Enums.MessageBoxAbstraction.MessageBoxResult.OK,
                MessageBoxResult.Cancel => Model.Enums.MessageBoxAbstraction.MessageBoxResult.Cancel,
                MessageBoxResult.Yes => Model.Enums.MessageBoxAbstraction.MessageBoxResult.Yes,
                MessageBoxResult.No => Model.Enums.MessageBoxAbstraction.MessageBoxResult.No,
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
            };
        }
    }
}
