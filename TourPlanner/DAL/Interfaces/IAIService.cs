using TourPlanner.Model.Enums;

namespace TourPlanner.DAL.Interfaces;

public interface IAiService
{
    Task<string> AnswerQueryAsync(string query, AiModel model);
}