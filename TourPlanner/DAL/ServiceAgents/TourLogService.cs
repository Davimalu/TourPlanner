using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using TourPlanner.config.Interfaces;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Model.Exceptions;
using TourPlanner.Models;

namespace TourPlanner.DAL.ServiceAgents
{
    public class TourLogService : ITourLogService
    {
        private readonly HttpClient _httpClient;
        private readonly ILoggerWrapper _logger;

        public TourLogService(HttpClient httpClient, ITourPlannerConfig tourPlannerConfig)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpClient.BaseAddress = new Uri(tourPlannerConfig.ApiBaseUrl);

            _logger = LoggerFactory.GetLogger<TourLogService>();
        }
        
        public async Task<List<TourLog>> GetTourLogsAsync(int tourId)
        {
            _logger.Debug($"Fetching all tour logs for Tour {tourId} from API...");

            try
            {
                // GetFromJsonAsync is a convenient method that handles the HTTP GET request and deserialization
                var tourLogs = await _httpClient.GetFromJsonAsync<List<TourLog>>($"/api/tours/{tourId}/logs");

                // If the API returns null, return an empty list
                var result = tourLogs ?? new List<TourLog>();

                _logger.Info($"Received {result.Count} tour logs from API");
                return result;
            }
            catch (HttpRequestException ex)
            {
                throw new ApiServiceException("Failed to get tour logs from API.", ex.StatusCode ?? HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<TourLog?> GetTourLogByIdAsync(int logId)
        {
            _logger.Debug($"Fetching tour log with ID {logId} from API...");
            
            try
            {
                return await _httpClient.GetFromJsonAsync<TourLog>($"/api/tours/logs/{logId}");
            }
            catch (HttpRequestException ex)
            {
                throw new ApiServiceException($"Failed to get tour log with ID {logId} from API.", ex.StatusCode ?? HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<TourLog?> CreateTourLogAsync(int tourId, TourLog newLog)
        {
            _logger.Debug($"Creating new tour log for tour with ID {tourId}...");
            
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"/api/tours/{tourId}/logs", newLog);

            if (response.IsSuccessStatusCode)
            {
                TourLog? createdLog = await response.Content.ReadFromJsonAsync<TourLog>();
                _logger.Info($"Created tour log with ID {createdLog?.LogId}: {createdLog?.Comment}");
                return createdLog;
            }
            
            await HandleFailedResponse(response, $"Failed to create tour log for tour with ID {tourId}.");
            return null;
        }

        public async Task<TourLog?> UpdateTourLogAsync(TourLog updatedLog)
        {
            _logger.Debug($"Updating tour log with ID {updatedLog.LogId}...");
            
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"/api/tours/logs/{updatedLog.LogId}", updatedLog);

            if (response.IsSuccessStatusCode)
            {
                TourLog? updatedTourLog = await response.Content.ReadFromJsonAsync<TourLog>();
                _logger.Info($"Updated tour log with ID {updatedTourLog?.LogId}: {updatedTourLog?.Comment}");
                return updatedTourLog;
            }
            
            await HandleFailedResponse(response, $"Failed to update tour log with ID {updatedLog.LogId}.");
            return null;
        }

        public async Task<bool> DeleteTourLogAsync(int logId)
        {
            _logger.Debug($"Deleting tour log with ID {logId}...");
            
            HttpResponseMessage response = await _httpClient.DeleteAsync($"/api/tours/logs/{logId}");

            if (response.IsSuccessStatusCode)
            {
                _logger.Info($"Deleted tour log with ID {logId}");
                return true;
            }
            
            await HandleFailedResponse(response, $"Failed to delete tour log with ID {logId}.");
            return false;
        }
        
        
        private async Task HandleFailedResponse(HttpResponseMessage response, string errorMessage)
        {
            var responseContent = await response.Content.ReadAsStringAsync(); 
    
            _logger.Error($"{errorMessage} Status: {response.StatusCode}. Response: {responseContent}");
        
            throw new ApiServiceException(errorMessage, response.StatusCode, responseContent);
        }
    }
}
