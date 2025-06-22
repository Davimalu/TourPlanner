using TourPlanner.Model;

namespace TourPlanner.Logic.Interfaces;

public interface IPdfService
{
    Task<bool> ExportTourAsPdfAsync(Tour tour, string filePath);
}