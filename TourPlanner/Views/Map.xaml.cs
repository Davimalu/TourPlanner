using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using TourPlanner.Logic.Interfaces;

namespace TourPlanner.Views
{
    public partial class Map : UserControl
    {
        private readonly IWebViewService _webViewService;
        private bool _isInitialized = false;
        
        public Map()
        {
            InitializeComponent();
            
            // Get the WebViewService from the service provider
            _webViewService = App.ServiceProvider.GetRequiredService<IWebViewService>();
            
            Loaded += Map_Loaded;
        }
        
        // This code is completely specific to WPF and WebView2. Thus, we think it's okay to put it in the Code Behind.
        private async void Map_Loaded(object? sender, RoutedEventArgs e)
        {
            // It seems that the WPF UI Tab Control loads the content of all tabs on startup and then again each time the tab is selected
            // Thus we have to make sure that the initialization code is only executed once (running it multiple times doesn't break things, but it's inefficient and unnecessary).
            if (_isInitialized)
            {
                return;
            }
            _isInitialized = true;
            
            try
            {
                // Set the WebView2 control in the service so that it can be used by the ViewModel and ensure the WebView2 control is initialized before proceeding
                // From an architectural perspective, this is a bit questionable, but since the WebViewService is so tightly coupled to the WebView2 control, we think it's okay to do it this way
                await _webViewService.InitializeAsync(WebView);
                
                // Construct the path to the folder containing the map resources
                var mapFolderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
                
                // Redirect all requests to the "appassets" domain to the local folder (instead of the "real" internet)
                WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    hostName: "appassets",
                    folderPath: mapFolderPath,
                    accessKind: Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow
                );
                
                // Navigate to the local HTML file that contains the map
                WebView.CoreWebView2.Navigate("https://appassets/map.html");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing WebView: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}