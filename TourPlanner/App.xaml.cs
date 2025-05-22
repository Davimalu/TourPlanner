using System.Configuration;
using System.Data;
using System.Windows;
using TourPlanner.Logic;
using TourPlanner.Logic.Interfaces;
using TourPlanner.ViewModels;
using TourPlanner.Views;

namespace TourPlanner;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // Services
        ISelectedTourService selectedTourService = new SelectedTourService();

        // Create the ViewModels
        MenuBarViewModel menuBarViewModel = new MenuBarViewModel();
        SearchBarViewModel searchBarViewModel = new SearchBarViewModel();
        TourListViewModel tourListViewModel = new TourListViewModel(selectedTourService);
        TourLogsViewModel tourLogsViewModel = new TourLogsViewModel(selectedTourService);
        TourDetailsViewModel tourDetailsViewModel = new TourDetailsViewModel(selectedTourService);
        MapViewModel mapViewModel = new MapViewModel(selectedTourService);
        MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
        
        Views.Map mapView = new Views.Map
        {
            DataContext = mapViewModel // Set its DataContext
        };

        // Create the Views (and initialize them with the ViewModels)
        MainWindow mainWindow = new MainWindow
        {
            DataContext = mainWindowViewModel,
            MenuBar = { DataContext = menuBarViewModel },
            SearchBar = { DataContext = searchBarViewModel },
            TourList = { DataContext = tourListViewModel },
            TourDetails = { DataContext = tourDetailsViewModel },
            Map = { Content = mapView }, // <--- CORRECTED: Assign the mapView instance
            TourLogs = { DataContext = tourLogsViewModel }
        };

        // Show the MainWindow
        mainWindow.Show();
    }
}

