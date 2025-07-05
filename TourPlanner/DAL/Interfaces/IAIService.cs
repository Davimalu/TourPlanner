using TourPlanner.Model.Enums;

namespace TourPlanner.DAL.Interfaces;

public interface IAiService
{
    Task<string> AnswerQueryAsync(string systemPrompt, string query, AiModel model);
}