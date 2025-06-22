using System.Globalization;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;

namespace TourPlanner.Logic;

public class SearchService : ISearchService
{
    private ILoggerWrapper _logger;
    
    public SearchService()
    {
        _logger = LoggerFactory.GetLogger<SearchService>();
    }
    
    /// <summary>
    /// Performs a full-text search on the list of tours (and their logs) based on the provided query
    /// </summary>
    /// <param name="query">The search query string</param>
    /// <param name="tours">The list of tours to search through</param>
    /// <returns>A list of tours (including their logs) that match the search query</returns>
    public async Task<List<Tour>> SearchToursAsync(string query, List<Tour> tours)
    {
        if (string.IsNullOrWhiteSpace(query) || tours.Count == 0)
        {
            _logger.Debug("Search query is empty or tours list is null/empty. Returning original list.");
            return tours;
        }
        
        // Run the search operation on a separate thread to avoid blocking the UI
        return await Task.Run(() =>
        {
            _logger.Debug($"Starting full text search with query: {query}");

            var lowerQuery = query.ToLowerInvariant();

            // We use the current culture to ensure that number formats are consistent with the user's locale
            return tours.Where(tour =>
                    tour.TourName.ToLowerInvariant().Contains(lowerQuery) ||
                    tour.TourDescription.ToLowerInvariant().Contains(lowerQuery) ||
                    tour.StartLocation.ToLowerInvariant().Contains(lowerQuery) ||
                    tour.EndLocation.ToLowerInvariant().Contains(lowerQuery) ||
                    tour.TransportationType.ToString().ToLowerInvariant().Contains(lowerQuery) ||
                    tour.Distance.ToString(CultureInfo.CurrentCulture).Contains(lowerQuery) ||
                    tour.EstimatedTime.ToString(CultureInfo.CurrentCulture).Contains(lowerQuery) ||
                    tour.Popularity.ToString(CultureInfo.CurrentCulture).Contains(lowerQuery) ||
                    tour.ChildFriendlyRating.ToString(CultureInfo.CurrentCulture).Contains(lowerQuery) ||
                    tour.AiSummary.ToLowerInvariant().Contains(lowerQuery) ||
                    tour.Logs.Any(log =>
                        log.TimeStamp.ToString(CultureInfo.CurrentCulture).Contains(lowerQuery) ||
                        log.Comment.ToLowerInvariant().Contains(lowerQuery) ||
                        log.Difficulty.ToString(CultureInfo.CurrentCulture).Contains(lowerQuery) ||
                        log.DistanceTraveled.ToString(CultureInfo.CurrentCulture).Contains(lowerQuery) ||
                        log.TimeTaken.ToString(CultureInfo.CurrentCulture).Contains(lowerQuery) ||
                        log.Rating.ToString(CultureInfo.CurrentCulture).Contains(lowerQuery)))
                .ToList();
        }).ConfigureAwait(false);
    }
}