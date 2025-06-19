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
        services.AddSingleton<IWindowService, WindowService>();
        
        services.AddTransient<HttpClient>();
        
        // DAL Services
        services.AddSingleton<ITourService, TourService>();
        services.AddSingleton<ITourLogService, TourLogService>();
        
        // ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MenuBarViewModel>();
        services.AddSingleton<SearchBarViewModel>();
        services.AddSingleton<TourListViewModel>();
        services.AddSingleton<TourDetailsViewModel>();
        services.AddSingleton<TourLogsViewModel>();
    }
    
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // Services
        ISelectedTourService selectedTourService = new SelectedTourService();

        // Create the ViewModels
        MapViewModel mapViewModel = new MapViewModel(selectedTourService); // ViewModel for the main map tab
        
        // FIX: DO NOT create a separate MapView instance here.
        // The MainWindow.xaml already defines the Map control. We just need to give it the right ViewModel.

        // Create the Views (and initialize them with the ViewModels)
        MainWindow mainWindow = new MainWindow
        {
            DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>(),
            MenuBar = { DataContext = ServiceProvider.GetRequiredService<MenuBarViewModel>() },
            SearchBar = { DataContext = ServiceProvider.GetRequiredService<SearchBarViewModel>() },
            TourList = { DataContext = ServiceProvider.GetRequiredService<TourListViewModel>() },
            TourDetails = { DataContext = ServiceProvider.GetRequiredService<TourDetailsViewModel>() },
            // FIX: Set the DataContext of the existing Map control in MainWindow
            Map = { DataContext = mapViewModel }, 
            TourLogs = { DataContext = ServiceProvider.GetRequiredService<TourLogsViewModel>() }
        };

        // Show the MainWindow
        mainWindow.Show();
    }
}