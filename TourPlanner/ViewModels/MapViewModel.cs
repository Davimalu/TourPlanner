using Microsoft.Web.WebView2.Wpf;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Models; // <--- IMPORTANT: Assumes Tour class is in TourPlanner.Model namespace

namespace TourPlanner.ViewModels
{
    public class MapViewModel : INotifyPropertyChanged
    {
        private readonly ISelectedTourService _selectedTourService;
        private WebView2 _webViewControl;
        private bool _isWebViewInitialized = false;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MapViewModel(ISelectedTourService selectedTourService)
        {
            _selectedTourService = selectedTourService ?? throw new ArgumentNullException(nameof(selectedTourService));
            _selectedTourService.SelectedTourChanged += OnSelectedTourChanged; // Subscribe to event
            System.Diagnostics.Debug.WriteLine("MapViewModel: Instance created.");
        }

        public async Task InitializeWebViewAsync(WebView2 webView)
        {
            if (webView == null)
            {
                System.Diagnostics.Debug.WriteLine("MapViewModel: InitializeWebViewAsync called with null WebView.");
                return;
            }

            _webViewControl = webView;
            System.Diagnostics.Debug.WriteLine("MapViewModel: WebView control received.");

            try
            {
                await _webViewControl.EnsureCoreWebView2Async(null);
                System.Diagnostics.Debug.WriteLine("MapViewModel: CoreWebView2 initialization ensured.");

                if (_webViewControl.CoreWebView2 != null)
                {
                    _isWebViewInitialized = true;
                    System.Diagnostics.Debug.WriteLine("MapViewModel: WebView is now initialized and ready.");

                    // Check for an initially selected tour
                    Tour? initialTour = _selectedTourService.CurrentSelectedTour;
                    if (initialTour != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"MapViewModel: Initial tour '{initialTour?.Name ?? "N/A"}' found, updating map.");
                        UpdateMapForTour(initialTour);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("MapViewModel: No initial tour selected, loading default map.");
                        LoadDefaultMap();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("MapViewModel: CoreWebView2 is null after EnsureCoreWebView2Async.");
                    _isWebViewInitialized = false;
                }
            }
            catch (Exception ex)
            {
                _isWebViewInitialized = false;
                System.Diagnostics.Debug.WriteLine($"MapViewModel: Error during WebView initialization: {ex.Message}");
                // Consider showing an error to the user via a message service or property binding
            }
        }

        private void LoadDefaultMap()
        {
            if (!_isWebViewInitialized || _webViewControl?.CoreWebView2 == null)
            {
                System.Diagnostics.Debug.WriteLine("MapViewModel: Cannot load default map, WebView not ready or CoreWebView2 is null.");
                return;
            }

            try
            {
                _webViewControl.CoreWebView2.Navigate("https://www.openstreetmap.org");
                System.Diagnostics.Debug.WriteLine("MapViewModel: Navigated to OpenStreetMap (Default Map).");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MapViewModel: Error navigating to default map: {ex.Message}");
            }
        }

        private void OnSelectedTourChanged(Tour? selectedTour)
        {
            System.Diagnostics.Debug.WriteLine($"MapViewModel: SelectedTourChanged event. New tour: {(selectedTour?.Name ?? "None")}");
            if (selectedTour != null)
            {
                UpdateMapForTour(selectedTour);
            }
            else
            {
                // If no tour is selected, revert to the default map view
                LoadDefaultMap();
            }
        }

        public void UpdateMapForTour(Tour tour) // tour is not nullable here due to check in OnSelectedTourChanged
        {
            if (!_isWebViewInitialized || _webViewControl?.CoreWebView2 == null)
            {
                System.Diagnostics.Debug.WriteLine($"MapViewModel: Cannot update map for tour '{tour?.Name ?? "N/A"}', WebView not ready or CoreWebView2 is null.");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"MapViewModel: Updating map for tour '{tour?.Name ?? "N/A"}'. From: {tour?.From ?? "N/A"}, To: {tour?.To ?? "N/A"}");

            try
            {
                // --- Actual Map Update Logic ---
                // This is a placeholder. You need to replace this with logic that
                // uses your tour's data (e.g., start/end coordinates, route polyline)
                // to display the tour on the map, likely by generating a URL
                // or executing JavaScript in the WebView2 control.

                string mapUrl;
                // Example: If you have coordinates in your Tour model
                // (Ensure properties like StartLat, StartLon, EndLat, EndLon exist and are populated)
                if (tour.FromLat != 0 && tour.FromLon != 0 && tour.ToLat != 0 && tour.ToLon != 0) // Basic check for valid coordinates
                {
                    // OSM example: https://www.openstreetmap.org/directions?engine=osrm_car&route=FROM_LAT%2CFROM_LON%3BTO_LAT%2CTO_LON
                    // Note: Lat/Lon need to be culture-invariant strings (e.g., using InvariantCulture for ToString)
                    string fromLatStr = tour.FromLat.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    string fromLonStr = tour.FromLon.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    string toLatStr = tour.ToLat.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    string toLonStr = tour.ToLon.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    mapUrl = $"https://www.openstreetmap.org/directions?engine=osrm_car&route={fromLatStr}%2C{fromLonStr}%3B{toLatStr}%2C{toLonStr}";
                }
                else if (!string.IsNullOrWhiteSpace(tour.To)) // Fallback to searching for the destination name
                {
                    string searchLocation = Uri.EscapeDataString(tour.To);
                    mapUrl = $"https://www.openstreetmap.org/search?query={searchLocation}";
                }
                else // Further fallback if no specific destination info
                {
                    mapUrl = "https://www.openstreetmap.org"; // Default map
                    System.Diagnostics.Debug.WriteLine($"MapViewModel: Tour '{tour?.Name ?? "N/A"}' has insufficient data for a specific map view, showing default.");
                }

                _webViewControl.CoreWebView2.Navigate(mapUrl);
                System.Diagnostics.Debug.WriteLine($"MapViewModel: Navigated for tour '{tour?.Name ?? "N/A"}'. URL: {mapUrl}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MapViewModel: Error navigating/updating map for tour '{tour?.Name ?? "N/A"}': {ex.Message}");
            }
        }
    }
}