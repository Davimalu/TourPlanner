using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Models;

namespace TourPlanner.DAL.ServiceAgents
{
    public class TourLogService
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

        public async Task<List<TourLogService>?> GetTourLogsAsync(int tourId)
        {
            _logger.Debug("Fetching all tour logs from API...");
            HttpResponseMessage response = await _httpClient.GetAsync($"/api/tours/{tourId}/logs");

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                List<TourLogService>? tourLogList = JsonSerializer.Deserialize<List<TourLogService>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.Info($"Received {tourLogList?.Count ?? '0'} tour logs from API");

                return tourLogList;
            }
            else
            {
                throw new Exception("Failed to get tour logs from API");
            }
        }

        public async Task<TourLog?> GetTourLogById(int logId)
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
                throw new Exception("Failed to get tour log from API");
            }
        }

        public async Task<TourLog?> CreateTourLogAsnyc(int tourId, TourLog newLog)
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
                throw new Exception("Failed to create tour log");
            }
        }

        public async Task<TourLog?> UpdateTourLogAsync(int logId, TourLog updatedLog)
        {
            _logger.Debug($"Updating tour log with ID {logId}...");

            string json = JsonSerializer.Serialize(updatedLog);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync($"/api/tours/logs/{logId}", content);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                TourLog? updatedTourLog = JsonSerializer.Deserialize<TourLog>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.Info($"Updated tour log with ID {updatedTourLog?.LogId}: {updatedTourLog?.Comment}");

                return updatedTourLog;
            }
            else
            {
                throw new Exception("Failed to update tour log");
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
                throw new Exception("Failed to delete tour log");
            }
        }
    }
}
