using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Models;

namespace TourPlanner.DAL.ServiceAgents
{
    class TourService
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

        public async Task<List<Tour>> GetToursAsync()
        {
            _logger.Debug("Fetching tours from API...");
            HttpResponseMessage response = await _httpClient.GetAsync("/api/Tours");

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                List<Tour> tourList = JsonSerializer.Deserialize<List<Tour>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Tour>();
                _logger.Info($"Received {tourList.Count} tours from API");

                return tourList;
            }
            else
            {
                throw new Exception("Failed to get tours from API");
            }
        }
    }
}
