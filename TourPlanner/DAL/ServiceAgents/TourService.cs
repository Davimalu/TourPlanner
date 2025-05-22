using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Model;
using TourPlanner.Models;

namespace TourPlanner.DAL.ServiceAgents
{
    class TourService : ITourService
    {
        private readonly string _baseUrl = "http://localhost:5168";
        private readonly HttpClient _httpClient;

        private readonly ILoggerWrapper _logger;

        public TourService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);

            _logger = LoggerFactory.GetLogger<TourService>();
        }

        public async Task<List<Tour>?> GetToursAsync()
        {
            _logger.Debug("Fetching all tours from API...");
            HttpResponseMessage response = await _httpClient.GetAsync("/api/Tours");

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                List<Tour>? tourList = JsonSerializer.Deserialize<List<Tour>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.Info($"Received {tourList?.Count ?? '0'} tours from API");

                return tourList;
            }
            else
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to get tours from API. Status Code: {(int)response.StatusCode} ({response.ReasonPhrase}). Response Body: {responseBody} ");
            }
        }

        public async Task<Tour?> GetTourByIdAsync(int id)
        {
            _logger.Debug($"Fetching tour with ID {id} from API...");
            HttpResponseMessage response = await _httpClient.GetAsync($"/api/Tours/{id}");

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                Tour? tour = JsonSerializer.Deserialize<Tour>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.Info($"Received tour with ID {tour?.TourId}: {tour?.TourName} from API");

                return tour;
            }
            else
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to get tour with ID {id} from API. Status Code: {(int)response.StatusCode} ({response.ReasonPhrase}). Response Body: {responseBody}");
            }
        }

        public async Task<Tour?> CreateTourAsync(Tour tour)
        {
            _logger.Debug($"Creating new tour: {tour.TourName}...");

            string json = JsonSerializer.Serialize(tour);
            StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("/api/Tours", content);

            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync();
                Tour? createdTour = JsonSerializer.Deserialize<Tour>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.Info($"Created new tour with ID {createdTour?.TourId}: {createdTour?.TourName}");

                return createdTour;
            }
            else
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create tour. Status Code: {(int)response.StatusCode} ({response.ReasonPhrase}). Response Body: {responseBody}");
            }
        }

        public async Task<Tour?> UpdateTourAsync(Tour tour)
        {
            _logger.Debug($"Updating tour with ID {tour.TourId}: {tour.TourName}...");

            string json = JsonSerializer.Serialize(tour);
            StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync($"/api/Tours/{tour.TourId}", content);

            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync();
                Tour? updatedTour = JsonSerializer.Deserialize<Tour>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.Info($"Updated tour with ID {updatedTour?.TourId}: {updatedTour?.TourName}");

                return updatedTour;
            }
            else
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to update tour with ID {tour.TourId}. Status Code: {(int)response.StatusCode} ({response.ReasonPhrase}). Response Body: {responseBody}");
            }
        }

        public async Task<bool> DeleteTourAsync(int id)
        {
            _logger.Debug($"Deleting tour with ID {id}...");

            HttpResponseMessage response = await _httpClient.DeleteAsync($"/api/Tours/{id}");

            if (response.IsSuccessStatusCode)
            {
                _logger.Info($"Deleted tour with ID {id}");
                return true;
            }
            else
            {
                _logger.Error($"Failed to delete tour with ID {id}");
                return false;
            }
        }
    }
}
