using System.Net.Http;
using System.Text;
using System.Text.Json;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Models;

namespace TourPlanner.DAL.ServiceAgents
{
    public class TourLogService : ITourLogService
    {
        private readonly string _baseUrl = "http://localhost:5168";
        private readonly HttpClient _httpClient;

        private readonly ILoggerWrapper _logger;

        public TourLogService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);

            _logger = LoggerFactory.GetLogger<TourLogService>();
        }

        public async Task<List<TourLog>?> GetTourLogsAsync(int tourId)
        {
            _logger.Debug("Fetching all tour logs from API...");
            HttpResponseMessage response = await _httpClient.GetAsync($"/api/tours/{tourId}/logs");

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                List<TourLog>? tourLogList = JsonSerializer.Deserialize<List<TourLog>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.Info($"Received {tourLogList?.Count ?? '0'} tour logs from API");

                return tourLogList;
            }
            else
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to get tour logs from API. Status Code: {(int)response.StatusCode} ({response.ReasonPhrase}). Response Body: {responseBody} ");
            }
        }

        public async Task<TourLog?> GetTourLogByIdAsync(int logId)
        {
            _logger.Debug($"Fetching tour log with ID {logId} from API...");
            HttpResponseMessage response = await _httpClient.GetAsync($"/api/tours/logs/{logId}");

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                TourLog? tourLog = JsonSerializer.Deserialize<TourLog>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.Info($"Received tour log with ID {tourLog?.LogId}: {tourLog?.Comment} from API");

                return tourLog;
            }
            else
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to get tour log from API. Status Code: {(int)response.StatusCode} ({response.ReasonPhrase}). Response Body: {responseBody} ");
            }
        }

        public async Task<TourLog?> CreateTourLogAsync(int tourId, TourLog newLog)
        {
            _logger.Debug($"Creating new tour log for tour with ID {tourId}...");

            string json = JsonSerializer.Serialize(newLog);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync($"/api/tours/{tourId}/logs", content);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                TourLog? createdLog = JsonSerializer.Deserialize<TourLog>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.Info($"Created tour log with ID {createdLog?.LogId}: {createdLog?.Comment}");

                return createdLog;
            }
            else
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create tour log. Status Code: {(int)response.StatusCode} ({response.ReasonPhrase}). Response Body: {responseBody} ");
            }
        }

        public async Task<TourLog?> UpdateTourLogAsync(TourLog updatedLog)
        {
            _logger.Debug($"Updating tour log with ID {updatedLog.LogId}...");

            string json = JsonSerializer.Serialize(updatedLog);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync($"/api/tours/logs/{updatedLog.LogId}", content);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                TourLog? updatedTourLog = JsonSerializer.Deserialize<TourLog>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.Info($"Updated tour log with ID {updatedTourLog?.LogId}: {updatedTourLog?.Comment}");

                return updatedTourLog;
            }
            else
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to update tour log. Status Code: {(int)response.StatusCode} ({response.ReasonPhrase}). Response Body: {responseBody} ");
            }
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
            else
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to delete tour log. Status Code: {(int)response.StatusCode} ({response.ReasonPhrase}). Response Body: {responseBody} ");
            }
        }
    }
}
