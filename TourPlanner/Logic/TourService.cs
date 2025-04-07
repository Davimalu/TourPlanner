using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TourPlanner.Models;

namespace TourPlanner.Logic
{
    class TourService
    {
        private readonly string _baseUrl = "http://localhost:5168";
        private readonly HttpClient _httpClient;

        public TourService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        public async Task<List<Tour>> GetToursAsync()
        {
            Debug.WriteLine("GetToursAsync: Sending GET request...");
            HttpResponseMessage response = await _httpClient.GetAsync("/api/Tours");
            Debug.WriteLine("GetToursAsync: Received response.");

            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine("GetToursAsync: Reading content...");
                string json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("GetToursAsync: Deserializing JSON...");
                List<Tour> tourList = JsonSerializer.Deserialize<List<Tour>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Debug.WriteLine($"GetToursAsync: Deserialized {tourList.Count} tours.");

                return tourList;
            }
            else
            {
                throw new Exception("Failed to get tours from API");
            }
        }
    }
}
