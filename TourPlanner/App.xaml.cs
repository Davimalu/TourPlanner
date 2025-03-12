using System.Configuration;
using System.Data;
using System.Windows;
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
        // Create the ViewModels
        SearchBarViewModel searchBarViewModel = new SearchBarViewModel();
        MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();

        // Create the Views (and initialize them with the ViewModels)
        MainWindow mainWindow = new MainWindow
        {
            DataContext = mainWindowViewModel,
            SearchBar = { DataContext = searchBarViewModel }
        };

        // Show the MainWindow
        mainWindow.Show();
    }
}

