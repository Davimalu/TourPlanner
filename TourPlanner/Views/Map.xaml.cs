// FILE: TourPlanner\Views\Map.xaml.cs

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
            // This event tells us when the WebView2 control's backend is ready.
            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
        }

        private async void WebView_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                MessageBox.Show($"WebView2 initialization failed. Error: {e.InitializationException?.Message}",
                    "WebView2 Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var mapFolder = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "MapResources");
            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                hostName: "appassets",
                folderPath: mapFolder,
                accessKind: Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow
            );
            if (this.DataContext is MapViewModel vm)
            {
                await vm.InitializeWebViewAsync(webView);
            }
        }

        // This handles the case where the DataContext is set *after* the WebView is already initialized.
        private async void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (webView != null && webView.CoreWebView2 != null && e.NewValue is MapViewModel vm)
            {
                await vm.InitializeWebViewAsync(webView);
            }
        }
    }
}