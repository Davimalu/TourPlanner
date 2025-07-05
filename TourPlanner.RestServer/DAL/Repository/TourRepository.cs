using Microsoft.EntityFrameworkCore;
using TourPlanner.RestServer.DAL.Repository.Interfaces;
using TourPlanner.Model;

namespace TourPlanner.RestServer.DAL.Repository;

public class TourRepository : ITourRepository
{
    private readonly ITourLogRepository _tourLogRepository;
    private readonly AppDbContext _context;
    
    
    public TourRepository(AppDbContext context, ITourLogRepository tourLogRepository)
    {
        _context = context;
        _tourLogRepository = tourLogRepository;
    }


    /// <summary>
    /// Asynchronously retrieves all tours, including their associated logs, from the database.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="Tour"/> objects, each containing its associated logs.</returns>
    public async Task<IEnumerable<Tour>> GetAllToursAsync()
    {
        var tours = await _context.Tours
            .Include(t => t.Logs)
            .ToListAsync();

        return tours;
    }


    /// <summary>
    /// Asynchronously retrieves a tour with the specified ID, including its associated logs, from the database.
    /// </summary>
    /// <param name="id">The ID of the tour to retrieve.</param>
    /// <returns>A <see cref="Tour"/> object containing the tour details and its associated logs.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no tour with the specified ID is found.</exception>
    public async Task<Tour> GetTourByIdAsync(int id)
    {
        var tour = await _context.Tours
            .Include(t => t.Logs)
            .FirstOrDefaultAsync(t => t.TourId == id);
        
        if (tour == null) 
        {
            throw new KeyNotFoundException($"Tour with ID {id} not found");
        }

        return tour;
    }


    /// <summary>
    /// Asynchronously adds a new tour to the database along with its associated logs.
    /// </summary>
    /// <param name="newTour">The new <see cref="Tour"/> object to be added to the database. Must include all required properties.</param>
    /// <returns>The newly added <see cref="Tour"/> object, including the auto-generated ID by the database.</returns>
    public async Task<Tour> AddTourAsync(Tour newTour)
    {
        // Clear IDs already associated with the tour and its logs to avoid database conflicts
        newTour.TourId = 0;
        foreach (var log in newTour.Logs)
        {
            log.LogId = 0;
        }
        
        _context.Tours.Add(newTour);
        await _context.SaveChangesAsync();
        
        return newTour; // The ID field is updated to the new auto-generated value automatically when calling SaveChangesAsync
    }


    /// <summary>
    /// Asynchronously updates an existing tour and its associated logs in the database.
    /// </summary>
    /// <param name="updatedTour">The updated <see cref="Tour"/> object containing the new data for the tour and its logs.</param>
    /// <returns>The updated <see cref="Tour"/> object, reflecting the changes applied to the database.</returns>
    public async Task<Tour> UpdateTourAsync(Tour updatedTour)
    { 
        // Retrieve the existing tour from the database, including its logs
        var tour = await _context.Tours
            .Include(t => t.Logs)
            .FirstOrDefaultAsync(t => t.TourId == updatedTour.TourId);
        
        if (tour == null)
        {
            throw new KeyNotFoundException($"Tour with ID {updatedTour.TourId} not found.");
        }

        // Update the tour's properties
        tour.TourName = updatedTour.TourName;
        tour.TourDescription = updatedTour.TourDescription;
        tour.StartLocation = updatedTour.StartLocation;
        tour.EndLocation = updatedTour.EndLocation;
        tour.TransportationType = updatedTour.TransportationType;
        tour.Distance = updatedTour.Distance;
        tour.EstimatedTime = updatedTour.EstimatedTime;
        tour.StartCoordinates = updatedTour.StartCoordinates;
        tour.EndCoordinates = updatedTour.EndCoordinates;
        tour.GeoJsonString = updatedTour.GeoJsonString;
        tour.Popularity = updatedTour.Popularity;
        tour.ChildFriendlyRating = updatedTour.ChildFriendlyRating;
        tour.AiSummary = updatedTour.AiSummary;

        // ------------------------------
        // Handle updating the TourLogs (simply doing tour.Logs = updatedTour.Logs; throws an exception)
        // ------------------------------

        // Remove logs that are no longer present in the updated tour
        var logsToRemove = tour.Logs
            .Where(existingLog => updatedTour.Logs.All(newLog => newLog.LogId != existingLog.LogId))
            .ToList();

        foreach (var log in logsToRemove)
        {
            await _tourLogRepository.DeleteTourLogAsync(log.LogId);
            
            // Remove the log from the local collection too (not necessary, but it doesn't hurt to ensure consistency)
            tour.Logs.Remove(log);
        }

        // Update all existing logs and add new logs
        foreach (var updatedLog in updatedTour.Logs)
        {
            // Check if the log already exists
            var existingLog = tour.Logs.FirstOrDefault(l => l.LogId == updatedLog.LogId);
            
            // Log already exists -> update it
            if (existingLog != null)
            {
                await _tourLogRepository.UpdateTourLogAsync(updatedLog);
            }
            // Log doesn't exist -> create it
            else
            {
                var addedLog = await _tourLogRepository.AddTourLogAsync(updatedTour.TourId, updatedLog);
                // Also add the new log to the local collection to ensure consistency
                tour.Logs.Add(updatedLog);
            }
        }

        // Save all changes.
        await _context.SaveChangesAsync();
        
        return tour;
    }


    /// <summary>
    /// Asynchronously deletes the tour with the specified ID, along with its associated logs, from the database.
    /// </summary>
    /// <param name="id">The unique identifier of the tour to be deleted.</param>
    /// <returns>A boolean value indicating whether the deletion was successful.</returns>
    public async Task<bool> DeleteTourAsync(int id)
    {
        var tour = await _context.Tours
            .Include(t => t.Logs)
            .FirstOrDefaultAsync(t => t.TourId == id);
        
        if (tour == null)
        {
            throw new KeyNotFoundException($"Tour with ID {id} not found.");
        }

        _context.Tours.Remove(tour);
        await _context.SaveChangesAsync();

        return true;
    }
}