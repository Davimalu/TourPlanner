using System.IO;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;

namespace TourPlanner.Logic;

public class PdfService : IPdfService
{
    private readonly ILoggerWrapper _logger;
    private readonly IMapService _mapService;
    
    public PdfService(IMapService mapService)
    {
        _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));

        _logger = LoggerFactory.GetLogger<PdfService>();
    }
    
    
    public async Task<bool> ExportTourAsPdfAsync(Tour tour, string filePath)
    {
        PdfWriter writer = new PdfWriter(filePath);
        PdfDocument pdfDocument = new PdfDocument(writer);
        Document document = new(pdfDocument, PageSize.A4, false);

        var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        document.SetMargins(20, 20, 20, 40);

        // Title
        var title = new Paragraph($"{tour.TourName}")
            .SetFont(boldFont)
            .SetFontSize(24)
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
            .SetMarginBottom(20);
        document.Add(title);
        
        // Route Image
        try
        {
            string imagePath = await GetRouteImagePathAsync(tour);
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                ImageData imageData = ImageDataFactory.Create(imagePath);
                Image routeImage = new Image(imageData);
                routeImage.SetWidth(UnitValue.CreatePercentValue(100));
                routeImage.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                routeImage.SetMarginBottom(20);
                document.Add(routeImage);
            }
        }
        catch (Exception ex)
        {
            // Log the error but continue with PDF generation
            _logger.Error($"Failed to add route image for tour '{tour.TourName}': {ex.Message}");
        }
        
        // Tour Details
        Table detailsTable = new Table(2);
        detailsTable.SetWidth(UnitValue.CreatePercentValue(100));
        
        AddDetailRow(detailsTable, "Description:", tour.TourDescription, font, boldFont);
        AddDetailRow(detailsTable, "Start Location:", tour.StartLocation, font, boldFont);
        AddDetailRow(detailsTable, "End Location:", tour.EndLocation, font, boldFont);
        AddDetailRow(detailsTable, "Means of transport:", tour.TransportationType.ToString(), font, boldFont);
        AddDetailRow(detailsTable, "Distance:", $"{tour.Distance:F2} km", font, boldFont);
        AddDetailRow(detailsTable, "Estimated Time:", $"{tour.EstimatedTime:F1} minutes", font, boldFont);
        
        AddDetailRow(detailsTable, "Popularity:", $"{tour.Popularity:F2} %", font, boldFont);
        AddDetailRow(detailsTable, "Child-Friendliness Rating:", $"{tour.ChildFriendlyRating:F2} %", font, boldFont);

        detailsTable.SetMarginBottom(10);
        document.Add(detailsTable);
        
        // Tour Logs
        foreach (var log in tour.Logs.OrderByDescending(l => l.TimeStamp))
        {
            // Log container
            var logContainer = new Div()
                .SetMarginBottom(15)
                .SetPadding(10)
                .SetBorder(new iText.Layout.Borders.SolidBorder(1));

            // Log header with timestamp
            var logHeader = new Paragraph($"Log Entry - {log.TimeStamp:dd/MM/yyyy HH:mm}")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetMarginBottom(8);
            logContainer.Add(logHeader);

            // Log details table
            Table logTable = new Table(2, false);
            logTable.SetWidth(UnitValue.CreatePercentValue(100));

            AddDetailRow(logTable, "Rating:", $"{log.Rating:F1}/5", font, boldFont);
            AddDetailRow(logTable, "Difficulty:", $"{log.Difficulty}/5", font, boldFont);
            AddDetailRow(logTable, "Distance Traveled:", $"{log.DistanceTraveled:F2} km", font, boldFont);
            AddDetailRow(logTable, "Time Taken:", $"{log.TimeTaken:F1} hours", font, boldFont);

            if (!string.IsNullOrWhiteSpace(log.Comment))
            {
                AddDetailRow(logTable, "Comment:", log.Comment, font, boldFont);
            }

            logContainer.Add(logTable);
            document.Add(logContainer);
        }

        // Summary statistics for logs
        if (tour.Logs.Count > 1)
        {
            var statsHeader = new Paragraph("Log Statistics")
                .SetFont(boldFont)
                .SetFontSize(16)
                .SetMarginTop(20)
                .SetMarginBottom(10);
            document.Add(statsHeader);

            Table statsTable = new Table(2, false);
            statsTable.SetWidth(UnitValue.CreatePercentValue(100));

            var avgRating = tour.Logs.Where(l => l.Rating > 0).Average(l => l.Rating);
            var avgDifficulty = tour.Logs.Average(l => l.Difficulty);
            var totalDistance = tour.Logs.Sum(l => l.DistanceTraveled);
            var totalTime = tour.Logs.Sum(l => l.TimeTaken);

            AddDetailRow(statsTable, "Total Logs:", tour.Logs.Count.ToString(), font, boldFont);
            AddDetailRow(statsTable, "Average Rating:", $"{avgRating:F1}/5", font, boldFont);
            AddDetailRow(statsTable, "Average Difficulty:", $"{avgDifficulty:F1}/5", font, boldFont);
            AddDetailRow(statsTable, "Total Distance Logged:", $"{totalDistance:F2} km", font, boldFont);
            AddDetailRow(statsTable, "Total Time Logged:", $"{totalTime:F1} hours", font, boldFont);

            document.Add(statsTable);
        }
        
        // AI Summary (if available)
        if (!string.IsNullOrWhiteSpace(tour.AiSummary))
        {
            var summaryHeader = new Paragraph("AI Summary")
                .SetFont(boldFont)
                .SetFontSize(16)
                .SetMarginTop(20)
                .SetMarginBottom(10);
            document.Add(summaryHeader);

            var summaryText = new Paragraph(tour.AiSummary)
                .SetFont(font)
                .SetFontSize(10)
                .SetMarginBottom(20);
            document.Add(summaryText);
        }
            
        document.Close();
        
        return File.Exists(filePath);
    }
    
    
    private void AddDetailRow(Table table, string label, string value, PdfFont font, PdfFont boldFont)
    {
        var labelCell = new Cell()
            .Add(new Paragraph(label).SetFont(boldFont).SetFontSize(12))
            .SetPadding(5)
            .SetWidth(UnitValue.CreatePercentValue(30));

        var valueCell = new Cell()
            .Add(new Paragraph(value).SetFont(font).SetFontSize(12))
            .SetPadding(5)
            .SetWidth(UnitValue.CreatePercentValue(70));

        table.AddCell(labelCell);
        table.AddCell(valueCell);
    }
    
    
    private async Task<string> GetRouteImagePathAsync(Tour tour)
    {
        return await _mapService.CaptureMapImageAsync(tour);
    }
}