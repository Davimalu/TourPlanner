using Newtonsoft.Json;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Model;

namespace TourPlanner.DAL.ServiceAgents;

/// <summary>
/// LocalTourService provides methods to save and load tours to and from local files
/// </summary>
public class LocalTourService : ILocalTourService
{
    private readonly ILogger<LocalTourService> _logger;
    
    public LocalTourService(ILogger<LocalTourService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    /// <summary>
    /// Saves a collection of Tour objects to a local file in JSON format
    /// </summary>
    /// <param name="tours">Collection of Tour objects to be saved</param>
    /// <param name="path">File path where the tours will be saved</param>
    /// <returns>True if the tours were saved successfully, false otherwise</returns>
    public async Task<bool> SaveToursToFileAsync(IEnumerable<Tour> tours, string path)
    {
        try
        {
            // Serialize the tours collection to JSON
            string jsonString = JsonConvert.SerializeObject(tours);
            
            // Write the serialized data to the specified file path
            await System.IO.File.WriteAllTextAsync(path, jsonString);
            
            _logger.Info($"Tours saved successfully to {path}");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Couldn't save tours to file: {ex.Message}");
            return false;
        }
    }


    /// <summary>
    /// Loads a collection of Tour objects from a local file in JSON format
    /// </summary>
    /// <param name="path">File path from which the tours will be loaded</param>
    /// <returns>Collection of Tour objects if loaded successfully, null if the file does not exist or is empty</returns>
    public async Task<IEnumerable<Tour>?> LoadToursFromFileAsync(string path)
    {
        try
        {
            // Check if the file exists
            if (!System.IO.File.Exists(path))
            {
                _logger.Warn($"Couldn't load tours from file: {path} does not exist");
                return null;
            }
            
            // Read JSON string from the specified file path
            string jsonString = await System.IO.File.ReadAllTextAsync(path);
            
            // Deserialize the JSON string back to a collection of Tour objects
            var tours = JsonConvert.DeserializeObject<List<Tour>>(jsonString);
            
            if (tours == null || tours.Count == 0)
            {
                _logger.Warn($"Couldn't load tours from file: {path} is empty or invalid");
                return null;
            }
            
            _logger.Info($"Tours loaded successfully from {path}");
            
            return tours;
        }
        catch (Exception ex)
        {
            _logger.Error($"Couldn't load tours from file: {ex.Message}");
            return null;
        }
    }
}