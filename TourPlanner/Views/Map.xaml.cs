using Microsoft.Web.WebView2.Core;
using System.Windows;
using System.Windows.Controls;
using TourPlanner.ViewModels;

namespace TourPlanner.Views
{
    public partial class Map : UserControl
    {
        public Map()
        {
            InitializeComponent();
            
            // The WPF Control for WebView2 is created instantly, but the underlying browser process (CoreWebView2) starts up asynchronously (and takes longer).
            // This event tells us when the WebView2 process is ready to use.
            WebView.CoreWebView2InitializationCompleted += HandleCoreWebView2Initialized;
        }
        
        // The following code is completely specific to WPF and WebView2. Thus, we think it's okay to put it in the Code Behind.

        private async void HandleCoreWebView2Initialized(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                MessageBox.Show($"WebView2 initialization failed. Error: {e.InitializationException?.Message}",
                    "WebView2 Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Construct the path to the folder containing the map resources
            var mapFolderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MapResources");
            
            // Redirect all requests to the "appassets" domain to the local folder (instead of the "real" internet)
            WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                hostName: "appassets",
                folderPath: mapFolderPath,
                accessKind: Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow
            );
            
            // Check if the DataContext was correctly set and call the method handling the rest of the logic
            if (this.DataContext is MapViewModel vm)
            {
                await vm.InitializeWebViewAsync(WebView);
            }
        }
    }
}