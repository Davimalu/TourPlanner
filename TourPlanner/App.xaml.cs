using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TourPlanner.config;
using TourPlanner.config.Interfaces;
using TourPlanner.DAL.Interfaces;
using TourPlanner.DAL.ServiceAgents;
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
    public static IServiceProvider ServiceProvider { get; private set; }
    
    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }
    
    private void ConfigureServices(IServiceCollection services)
    {
        // Services
        services.AddSingleton<ISelectedTourService, SelectedTourService>();
        services.AddSingleton<ITourPlannerConfig, TourPlannerConfig>();
        
        services.AddTransient<HttpClient>();
        
        // DAL Services
        services.AddSingleton<ITourService, TourService>();
        services.AddSingleton<ITourLogService, TourLogService>();
        
        // ViewModels
    }
    
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