using Microsoft.Web.WebView2.Core; // Required for CoreWebView2InitializationCompletedEventArgs
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TourPlanner.ViewModels; // Your MapViewModel namespace

namespace TourPlanner.Views
{
    public partial class Map : UserControl
    {
        private bool _isCoreWebView2Initialized = false;

        public Map()
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("Map.xaml.cs: Constructor called.");

            // Attach to the CoreWebView2InitializationCompleted event.
            // This is the most reliable way to know when WebView2 is ready.
            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;

            // Handle DataContextChanged to ensure ViewModel gets the WebView
            // if DataContext is set after the view is loaded but before CoreWebView2 is ready,
            // or if CoreWebView2 initializes before DataContext is set.
            this.DataContextChanged += Map_DataContextChanged;

            // It's also good practice to explicitly start initialization if it hasn't started automatically.
            // This call is non-blocking and will trigger CoreWebView2InitializationCompleted.
            // Do this after subscribing to the event.
            InitializeWebViewSource();
        }

        private async void InitializeWebViewSource()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Map.xaml.cs: Calling EnsureCoreWebView2Async...");
                // No need to set Source here if your ViewModel will navigate.
                // If you have a default Source you want to set before VM takes over, you can.
                await webView.EnsureCoreWebView2Async(null); // Pass null for default environment
                System.Diagnostics.Debug.WriteLine("Map.xaml.cs: EnsureCoreWebView2Async completed.");
                // The WebView_CoreWebView2InitializationCompleted event will handle the next steps.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Map.xaml.cs: Error in EnsureCoreWebView2Async: {ex.Message}");
                MessageBox.Show($"Failed to initialize WebView2 environment: {ex.Message}", "WebView2 Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void WebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                _isCoreWebView2Initialized = true;
                System.Diagnostics.Debug.WriteLine("Map.xaml.cs: CoreWebView2InitializationCompleted - Success.");
                // CoreWebView2 is ready. Now try to pass it to the ViewModel.
                await PassWebViewToViewModel();
            }
            else
            {
                _isCoreWebView2Initialized = false;
                System.Diagnostics.Debug.WriteLine($"Map.xaml.cs: CoreWebView2InitializationCompleted - Failed. Error: {e.InitializationException?.Message}");
                MessageBox.Show($"WebView2 could not be initialized. Error: {e.InitializationException?.Message}", "WebView2 Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Map_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Map.xaml.cs: DataContextChanged. New DataContext is { (e.NewValue == null ? "null" : e.NewValue.GetType().Name) }.");
            // DataContext (ViewModel) has changed.
            // If CoreWebView2 is already initialized, try to pass the WebView control.
            if (_isCoreWebView2Initialized && webView.CoreWebView2 != null)
            {
                await PassWebViewToViewModel();
            }
            else if (e.NewValue is MapViewModel && !_isCoreWebView2Initialized)
            {
                System.Diagnostics.Debug.WriteLine("Map.xaml.cs: DataContext set, but CoreWebView2 not yet initialized. Waiting for CoreWebView2InitializationCompleted.");
            }
        }

        private async Task PassWebViewToViewModel()
        {
            if (this.DataContext is MapViewModel vm && webView != null && _isCoreWebView2Initialized && webView.CoreWebView2 != null)
            {
                System.Diagnostics.Debug.WriteLine("Map.xaml.cs: Passing WebView to ViewModel.");
                await vm.InitializeWebViewAsync(webView);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Map.xaml.cs: Conditions not met to pass WebView to ViewModel.");
                if (!(this.DataContext is MapViewModel)) System.Diagnostics.Debug.WriteLine(" - DataContext is not MapViewModel or is null.");
                if (webView == null) System.Diagnostics.Debug.WriteLine(" - webView control is null.");
                if (!_isCoreWebView2Initialized) System.Diagnostics.Debug.WriteLine(" - CoreWebView2 is not initialized (_isCoreWebView2Initialized is false).");
                if (webView != null && webView.CoreWebView2 == null && _isCoreWebView2Initialized) System.Diagnostics.Debug.WriteLine(" - CoreWebView2 is null even after _isCoreWebView2Initialized is true (unexpected).");
            }
        }

        // Optional: If you need to do something when the UserControl itself is loaded.
        // private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        // {
        // System.Diagnostics.Debug.WriteLine("Map.xaml.cs: UserControl_Loaded.");
        // If CoreWebView2 is already initialized by this point AND DataContext is set,
        // this provides another chance to link them.
        // if (_isCoreWebView2Initialized && webView.CoreWebView2 != null)
        // {
        // await PassWebViewToViewModel();
        // }
        // }
    }
}