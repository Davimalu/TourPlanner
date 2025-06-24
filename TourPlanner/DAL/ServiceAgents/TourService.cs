using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using TourPlanner.config.Interfaces;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Exceptions;

namespace TourPlanner.DAL.ServiceAgents
{
    class TourService : ITourService
    {
        private readonly HttpClient _httpClient;

        private readonly ILogger<TourService> _logger;

        public TourService(HttpClient httpClient, ITourPlannerConfig tourPlannerConfig, ILogger<TourService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _httpClient.BaseAddress = new Uri(tourPlannerConfig.ApiBaseUrl);
        }

        public async Task<List<Tour>?> GetToursAsync()
        {
            _logger.Debug("Fetching all tours from API...");
            
            try
            {
                var tours = await _httpClient.GetFromJsonAsync<List<Tour>>("/api/Tours");
                
                // Return an empty list instead of null for collections
                var result = tours ?? new List<Tour>();
                
                _logger.Info($"Received {result.Count} tours from API");
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.Error($"Failed to get tours from API. Status: {ex.StatusCode}", ex);
                throw new ApiServiceException("Failed to get tours from API.", ex.StatusCode ?? HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<Tour?> GetTourByIdAsync(int id)
        {
            _logger.Debug($"Fetching tour with ID {id} from API...");
            
            try
            {
                return await _httpClient.GetFromJsonAsync<Tour>($"/api/Tours/{id}");
            }
            catch (HttpRequestException ex)
            {
                // For a 404 Not Found, we return null instead of throwing an exception
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.Warn($"Tour with ID {id} not found.");
                    return null;
                }
                
                _logger.Error($"Failed to get tour with ID {id}. Status: {ex.StatusCode}", ex);
                throw new ApiServiceException($"Failed to get tour with ID {id} from API.", ex.StatusCode ?? HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<Tour?> CreateTourAsync(Tour tour)
        {
            _logger.Debug($"Creating new tour: {tour.TourName}...");
            
            // PostAsJsonAsync handles serializing the 'tour' object
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/Tours", tour);
            
            if (response.IsSuccessStatusCode)
            {
                Tour? createdTour = await response.Content.ReadFromJsonAsync<Tour>();
                _logger.Info($"Created new tour with ID {createdTour?.TourId}: {createdTour?.TourName}");
                return createdTour;
            }
            
            await HandleFailedResponse(response, $"Failed to create tour with ID {tour.TourId}");
            return null; // Unreachable code
        }

        public async Task<Tour?> UpdateTourAsync(Tour tour)
        {
            _logger.Debug($"Updating tour with ID {tour.TourId}: {tour.TourName}...");
            
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"/api/Tours/{tour.TourId}", tour);

            if (response.IsSuccessStatusCode)
            {
                Tour? updatedTour = await response.Content.ReadFromJsonAsync<Tour>();
                _logger.Info($"Updated tour with ID {updatedTour?.TourId}: {updatedTour?.TourName}");
                return updatedTour;
            }
            
            await HandleFailedResponse(response, $"Failed to update tour with ID {tour.TourId}.");
            return null; // Unreachable code
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
            
            await HandleFailedResponse(response, $"Failed to delete tour with ID {id}.");
            return false; // Unreachable code
        }
        
        
        private async Task HandleFailedResponse(HttpResponseMessage response, string errorMessage)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            
            _logger.Error($"{errorMessage} Status: {response.StatusCode}. Response: {responseContent}");
            
            throw new ApiServiceException(errorMessage, response.StatusCode, responseContent);
        }
    }
}