using System.Text.Json;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Enums;

namespace TourPlanner.Logic;

public class AttributeService : IAttributeService
{
    private readonly ILoggerWrapper _logger;
    private readonly ITourService _tourService;
    private readonly IAiService _aiService;
    
    public AttributeService(ITourService tourService, IAiService aiService)
    {
        _tourService = tourService ?? throw new ArgumentNullException(nameof(tourService));
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _logger = LoggerFactory.GetLogger<AttributeService>();
    }
    
    // ---------------------------------------------
    // Parameters for child-friendliness calculation
    // ---------------------------------------------
    
    // The maximum difficulty rating on the scale.
    private const float MaxDifficultyRating = 5.0f;

    // We consider a tour increasingly unfriendly as it approaches this distance.
    // At or beyond this distance, the distance score becomes 0.
    private const float IdealMaxChildDistanceKm = 5.0f;

    // We consider a tour increasingly unfriendly as it approaches this duration.
    // At or beyond this duration, the duration score becomes 0.
    private const float IdealMaxChildDurationMin = 120.0f;
    
    // Weights for combining the normalized scores. Must sum to 1.0.
    private const float DifficultyWeight = 0.50f;
    private const float DurationWeight = 0.30f;
    private const float DistanceWeight = 0.20f;
    
    // ---------------------------------------------
    // Parameters for AI Summary generation
    // ---------------------------------------------
    private const string SystemPrompt = "You are an expert tour guide and analyst. Your task is to provide a concise summary of the tour details and its associated logs based on the provided details. Talk about the tour's highlights, challenges, and any notable features. Use the following JSON data to generate your summary:\n\n";
    
    
    /// <summary>
    /// Calculates the popularity of the tour based on the number of logs (relative to the most popular tour)
    /// </summary>
    /// <param name="tour">The tour for which to calculate popularity</param>
    /// <returns>Popularity score as a float</returns>
    public async Task<float> CalculatePopularityAsync(Tour tour)
    {
        // Get all tours from the tour service
        var allTours = await _tourService.GetToursAsync();
        
        if (allTours == null || !allTours.Any())
        {
            _logger.Warn("Failed to calculate popularity: No tours found.");
            return 0f;
        }
        
        int logCount = 0;
        int maxLogCount = 0;

        // Iterate through all tours to find the maximum log count of any tour
        foreach (var t in allTours)
        {
            int currentLogCount = t.Logs.Count;
            if (currentLogCount > maxLogCount)
            {
                maxLogCount = currentLogCount;
            }

            // If the current tour matches the one we're calculating for, get its log count
            if (t.TourId == tour.TourId)
            {
                logCount = currentLogCount;
            }
        }

        // Calculate popularity as a percentage of the maximum log count
        if (maxLogCount == 0)
        {
            _logger.Warn("Failed to calculate popularity: No logs found for any tours");
            return 0f;
        }

        float popularity = (float)logCount / maxLogCount * 100f; // Scale to percentage
        _logger.Debug($"Calculated popularity for tour '{tour.TourName}' with ID {tour.TourId}: {popularity}% based on it having {logCount} logs compared to the most popular tour with {maxLogCount} logs.");

        return popularity;
    }
    
    
    /// <summary>
    /// Calculates the child-friendliness of the tour based on the difficulty ratings, total times and distances of the tour logs
    /// </summary>
    /// <param name="tour">The tour for which to calculate child-friendliness</param>
    /// <returns>Child-friendliness score as a float</returns>
    public float CalulateChildFriendliness(Tour tour)
    {
        if (!tour.Logs.Any())
        {
            _logger.Warn($"Failed to calculate child-friendliness for tour '{tour.TourName}' with ID {tour.TourId}: No logs available.");
            return 0f; // No logs means no child-friendliness
        }
        
        double totalWeightedFriendlinessScore = 0;
        
        foreach (var log in tour.Logs)
        {
            // Calculate normalized scores for each metric of each log
            double difficultyScore = GetDifficultyScore(log.Difficulty);
            double distanceScore = GetDistanceScore(log.DistanceTraveled);
            double durationScore = GetDurationScore(log.TimeTaken);
            
            // Combine the scores using the defined weights
            double combinedScore = (difficultyScore * DifficultyWeight) +
                                   (distanceScore * DistanceWeight) +
                                   (durationScore * DurationWeight);
            
            // Add the weighted score to the total
            totalWeightedFriendlinessScore += combinedScore;
        }
        
        // Calculate the average score across all logs
        double averageFriendlinessScore = totalWeightedFriendlinessScore / tour.Logs.Count;
        
        // Ensure the score is between 0 and 1
        float childFriendlyRating = (float)Math.Max(0, Math.Min(1.0, averageFriendlinessScore));
        _logger.Debug($"Calculated child-friendliness for tour '{tour.TourName}' with ID {tour.TourId}: {childFriendlyRating * 100}% based on {tour.Logs.Count} logs.");
        
        return childFriendlyRating * 100f; // Return as percentage
    }


    private double GetDifficultyScore(float difficulty)
    {
        // Ensure difficulty is within the valid range
        difficulty = Math.Max(0, Math.Min(difficulty, MaxDifficultyRating));
        
        // Invert the score: 0 difficulty -> 1.0 score, 5 difficulty -> 0.0 score
        return (MaxDifficultyRating - difficulty) / MaxDifficultyRating;
    }
    
    private double GetDistanceScore(float distance)
    {
        if (distance <= 0)
        {
            return 1.0;
        }
        
        // Score decreases linearly until the ideal max distance
        double score = 1.0 - (distance / IdealMaxChildDistanceKm);
        return Math.Max(0, score); // Ensure score doesn't go below 0
    }
    
    private double GetDurationScore(float duration)
    {
        if (duration <= 0)
        {
            return 1.0;
        }
        
        // Score decreases linearly until the ideal max duration
        double score = 1.0 - (duration / IdealMaxChildDurationMin);
        return Math.Max(0, score); // Ensure score doesn't go below 0
    }


    /// <summary>
    /// Generates an AI summary for the given tour using the AI service
    /// </summary>
    /// <param name="tour">The tour for which to generate the summary</param>
    /// <returns>A summary of the tour as a string</returns>
    public async Task<string> GetAiSummaryAsync(Tour tour)
    {
        string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(tour);
        return await _aiService.AnswerQueryAsync(SystemPrompt + jsonString, AiModel.GPT4_1);
    }
}