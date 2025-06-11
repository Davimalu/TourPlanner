// FILE: TourPlanner\Views\Map.xaml.cs
using System;
using System.IO;
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
                MessageBox.Show($"WebView2 initialization failed. Error: {e.InitializationException?.Message}", "WebView2 Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string mapPath = Path.Combine(baseDirectory, "MapResources", "map.html");
            webView.CoreWebView2.Navigate(mapPath);

            // At this point, webView.CoreWebView2 is ready.
            // Check if our DataContext (the ViewModel) has been set.
            if (this.DataContext is MapViewModel vm)
            {
                // Pass the control to the ViewModel for initialization.
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