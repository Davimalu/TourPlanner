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
        MapViewModel mapViewModel = new MapViewModel(selectedTourService); // ViewModel for the main map tab
        MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
        
        // FIX: DO NOT create a separate MapView instance here.
        // The MainWindow.xaml already defines the Map control. We just need to give it the right ViewModel.

        // Create the Views (and initialize them with the ViewModels)
        MainWindow mainWindow = new MainWindow
        {
            DataContext = mainWindowViewModel,
            MenuBar = { DataContext = menuBarViewModel },
            SearchBar = { DataContext = searchBarViewModel },
            TourList = { DataContext = tourListViewModel },
            TourDetails = { DataContext = tourDetailsViewModel },
            // FIX: Set the DataContext of the existing Map control in MainWindow
            Map = { DataContext = mapViewModel }, 
            TourLogs = { DataContext = tourLogsViewModel }
        };

        // Show the MainWindow
        mainWindow.Show();
    }
}