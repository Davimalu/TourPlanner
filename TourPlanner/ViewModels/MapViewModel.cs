using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using TourPlanner.DAL.ServiceAgents;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;

namespace TourPlanner.ViewModels
{
    public class MapMessage
    {
        public string Type { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public class MapViewModel : INotifyPropertyChanged
    {
        private readonly ISelectedTourService? _selectedTourService;
        private readonly MapService _mapService = new MapService();
        private WebView2? _webViewControl;
        private bool _isWebViewReady = false; // Use a more descriptive name

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<MapClickEventArgs>? MapClicked;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MapViewModel(ISelectedTourService? selectedTourService)
        {
            _selectedTourService = selectedTourService;
            if (_selectedTourService != null)
            {
                _selectedTourService.SelectedTourChanged += OnSelectedTourChanged;
            }
        }
        
        
        public async Task InitializeWebViewAsync(WebView2 webView)
        {
            // Prevent re-initialization if the control has already been linked.
            if (_webViewControl != null) return; 

            _webViewControl = webView;
    
            // Ensure the CoreWebView2 backend is ready.
            await _webViewControl.EnsureCoreWebView2Async(null);
    
            if (_webViewControl.CoreWebView2 == null)
            {
                System.Diagnostics.Debug.WriteLine("MapViewModel: CoreWebView2 is NULL. Aborting communication setup.");
                return;
            }
    
            // *** THIS IS THE CRITICAL PART WE NEED TO RESTORE ***
            // Listen for messages from JavaScript (like map clicks).
            _webViewControl.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            // Listen for when the page has finished loading.
            _webViewControl.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;

            System.Diagnostics.Debug.WriteLine("MapViewModel: Communication with WebView is now established.");
    
            // Since the XAML's Source property already loaded the page, NavigationCompleted
            // might have already fired. We'll set the _isWebViewReady flag to true here to be safe,
            // so that subsequent calls to DrawRoute etc. will work.
            _isWebViewReady = true;

            // If a tour was selected before the map was fully ready, display its route now.
            if (_selectedTourService?.CurrentSelectedTour != null)
            {
                await DisplayTourRoute(_selectedTourService.CurrentSelectedTour); // Assuming CurrentTour is the property name
            }
        }
        
        private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var message = JsonSerializer.Deserialize<MapMessage>(e.WebMessageAsJson);
                if (message?.Type == "MapClick")
                {
                    MapClicked?.Invoke(this, new MapClickEventArgs(message.Lat, message.Lon));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error receiving web message: {ex.Message}");
            }
        }

        private async void OnSelectedTourChanged(Tour? selectedTour)
        {
            if (!_isWebViewReady) return; // Don't do anything if the map page isn't even loaded
            
            if (selectedTour != null)
            {
                await DisplayTourRoute(selectedTour);
            }
            else
            {
                ClearMap();
            }
        }
        
        private async Task DisplayTourRoute(Tour tour)
        {
            if (!_isWebViewReady) return;

            if (tour.StartLat == 0 || tour.StartLon == 0 || tour.EndLat == 0 || tour.EndLon == 0)
            {
                System.Diagnostics.Debug.WriteLine($"MapViewModel: Tour '{tour.TourName}' has no coordinates. Clearing map.");
                ClearMap();
                return;
            }

            var startPoint = (tour.StartLon, tour.StartLat);
            var endPoint = (tour.EndLon, tour.EndLat);

            System.Diagnostics.Debug.WriteLine($"MapViewModel: Displaying route for '{tour.TourName}'.");
            var routeInfo = await _mapService.GetRouteAsync(tour.TransportationType, startPoint, endPoint);

            if (routeInfo != null)
            {
                ClearMap();
                AddMarker(tour.StartLat, tour.StartLon, "start");
                AddMarker(tour.EndLat, tour.EndLon, "end");
                DrawRoute(routeInfo.RouteGeometry);
            }
        }
        
        public async void DrawRoute(string geoJson)
        {
            if (!_isWebViewReady) return;
            var escapedJson = HttpUtility.JavaScriptStringEncode(geoJson);
            await _webViewControl!.CoreWebView2.ExecuteScriptAsync($"drawRoute('{escapedJson}')");
        }
        
        public async void ClearMap()
        {
            if (!_isWebViewReady) return;
            await _webViewControl!.CoreWebView2.ExecuteScriptAsync("clearMap()");
        }
        
        
        
        private readonly Queue<string> _pendingScripts = new();
        public async void AddMarker(double lat, double lon, string type)
        {
            var script = $"addMarker({lat}, {lon}, '{type}')";
            if (!_isWebViewReady)
            {
                _pendingScripts.Enqueue(script);
                return;
            }
            await _webViewControl!.CoreWebView2.ExecuteScriptAsync(script);
        }

        private async void FlushPending()
        {
            while (_pendingScripts.TryDequeue(out var js))
                await _webViewControl!.CoreWebView2.ExecuteScriptAsync(js);
        }

        private void CoreWebView2_NavigationCompleted(
            object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            _isWebViewReady = e.IsSuccess;
            if (_isWebViewReady) FlushPending();   // no await
        }
        
    }
}




public class MapClickEventArgs : EventArgs
{
    public double Lat { get; }
    public double Lon { get; }
    public MapClickEventArgs(double lat, double lon)
    {
        Lat = lat;
        Lon = lon;
    }
}