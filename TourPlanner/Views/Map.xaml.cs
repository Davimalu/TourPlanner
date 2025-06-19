using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using TourPlanner.Logic.Interfaces;

namespace TourPlanner.Views
{
    public partial class Map : UserControl
    {
        private readonly IWebViewService _webViewService;
        
        public Map()
        {
            InitializeComponent();
            
            // Get the WebViewService from the ServiceProvider
            _webViewService = App.ServiceProvider.GetRequiredService<IWebViewService>();
            
            Loaded += Map_Loaded;
        }
        
        // This code is completely specific to WPF and WebView2. Thus, we think it's okay to put it in the Code Behind.
        private async void Map_Loaded(object? sender, RoutedEventArgs e)
        {
            try
            {
                // Set the WebView2 control in the service so that it can be used by the ViewModel
                _webViewService.SetWebView(WebView);
            
                // Ensure the WebView2 control is initialized before proceeding
                await _webViewService.InitializeAsync();
                
                // Construct the path to the folder containing the map resources
                var mapFolderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
                
                // Redirect all requests to the "appassets" domain to the local folder (instead of the "real" internet)
                WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    hostName: "appassets",
                    folderPath: mapFolderPath,
                    accessKind: Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing WebView: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}