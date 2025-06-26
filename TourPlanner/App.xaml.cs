using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TourPlanner.config;
using TourPlanner.config.Interfaces;
using TourPlanner.DAL.Interfaces;
using TourPlanner.DAL.ServiceAgents;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic;
using TourPlanner.Logic.Interfaces;
using TourPlanner.ViewModels;
using TourPlanner.Views;

namespace TourPlanner;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public static IServiceProvider? ServiceProvider { get; private set; }
    
    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }
    
    private void ConfigureServices(IServiceCollection services)
    {
        // Services
        services.AddSingleton<ITourPlannerConfig, TourPlannerConfig>();
        services.AddSingleton<IWpfService, WpfService>();
        services.AddSingleton<IMapService, MapService>();
        services.AddSingleton<IWebViewService, WebViewService>();
        services.AddSingleton<ISearchService, SearchService>();
        services.AddSingleton<IAttributeService, AttributeService>();
        services.AddSingleton<IPdfService, PdfService>();
        services.AddSingleton<IEventAggregator, EventAggregator>();
        
        // Infrastructure
        services.AddTransient(typeof(ILogger<>), typeof(Logger<>));
        services.AddSingleton<IFileSystemWrapper, FileSystemWrapper>();
        
        services.AddTransient<HttpClient>();
        
        // DAL Services
        services.AddSingleton<ITourService, TourService>();
        services.AddSingleton<ITourLogService, TourLogService>();
        services.AddSingleton<ILocalTourService, LocalTourService>();
        services.AddSingleton<IOrsService, OrsService>();
        services.AddSingleton<IAiService, AiService>();
        services.AddSingleton<IIoService, IoService>();
        
        // ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MenuBarViewModel>();
        services.AddSingleton<SearchBarViewModel>();
        services.AddSingleton<TourListViewModel>();
        services.AddSingleton<TourAttributesViewModel>();
        services.AddSingleton<TourDetailsViewModel>();
        services.AddSingleton<TourLogsViewModel>();
        services.AddSingleton<MapViewModel>();
    }
    
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        if (ServiceProvider == null)
        {
            throw new InvalidOperationException("ServiceProvider is not initialized. Ensure that the App constructor is called before Application_Startup.");
        }
        
        // Create the Views (and initialize them with the ViewModels)
        MainWindow mainWindow = new MainWindow
        {
            DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>(),
            MenuBar = { DataContext = ServiceProvider.GetRequiredService<MenuBarViewModel>() },
            SearchBar = { DataContext = ServiceProvider.GetRequiredService<SearchBarViewModel>() },
            TourList = { DataContext = ServiceProvider.GetRequiredService<TourListViewModel>() },
            TourDetails = { DataContext = ServiceProvider.GetRequiredService<TourDetailsViewModel>() },
            TourLogs = { DataContext = ServiceProvider.GetRequiredService<TourLogsViewModel>() },
            Map = { DataContext = ServiceProvider.GetRequiredService<MapViewModel>() },
            TourAttributes = { DataContext = ServiceProvider.GetRequiredService<TourAttributesViewModel>() }
        };

        // Show the MainWindow
        mainWindow.Show();
        
        // Initialize the Application with the light theme
        IWpfService wpfService = ServiceProvider.GetRequiredService<IWpfService>();
        wpfService.ApplyLightTheme();
    }
}