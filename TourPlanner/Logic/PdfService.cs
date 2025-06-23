using System.IO;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
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
    
    // ---------------------------
    // Detailed Tour PDF Export
    // ---------------------------

    // TODO: Refactor statistics calculation logic into a separate service for better separation of concerns; maybe then also display the statistics in the UI
    
    /// <summary>
    /// Exports the details of a single tour as a PDF document
    /// </summary>
    /// <param name="tour">The tour to export</param>
    /// <param name="filePath">The file path where the PDF will be saved</param>
    /// <returns>True if the PDF was successfully created, false otherwise</returns>
    public async Task<bool> ExportTourAsPdfAsync(Tour tour, string filePath)
    {
        var writer = new PdfWriter(filePath);
        var pdfDocument = new PdfDocument(writer);
        Document document = new(pdfDocument, PageSize.A4, false);

        var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        document.SetMargins(20, 20, 20, 40);

        // Title
        var title = new Paragraph($"{tour.TourName}")
            .SetFont(boldFont)
            .SetFontSize(24)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(20);
        document.Add(title);

        // Route Image
        await AddRouteImage(tour, document);

        // Tour Details
        AddTourDetails(tour, font, boldFont, document);

        // Tour Logs
        AddLogContainers(tour, boldFont, font, document);

        // Summary statistics for logs
        AddLogSummary(tour, boldFont, document, font);

        // AI Summary
        AddAiSummary(tour, boldFont, document, font);

        document.Close();

        return File.Exists(filePath);
    }

    
    /// <summary>
    /// Adds the summary statistics for logs to the PDF document for the given tour
    /// </summary>
    /// <param name="tour">Tour for which the summary should be added</param>
    /// <param name="boldFont">The font to use for bold text</param>
    /// <param name="document"> The PDF document to which the summary should be added</param>
    /// <param name="font">The font to use for normal text</param>
    private void AddLogSummary(Tour tour, PdfFont boldFont, Document document, PdfFont font)
    {
        if (tour.Logs.Count > 1)
        {
            var statsHeader = new Paragraph("Log Statistics")
                .SetFont(boldFont)
                .SetFontSize(16)
                .SetMarginTop(20)
                .SetMarginBottom(10);
            document.Add(statsHeader);

            var statsTable = new Table(2, false);
            statsTable.SetWidth(UnitValue.CreatePercentValue(100));

            var avgRating = tour.Logs.Where(l => l.Rating > 0).Average(l => l.Rating);
            var avgDifficulty = tour.Logs.Average(l => l.Difficulty);
            var totalDistance = tour.Logs.Sum(l => l.DistanceTraveled);
            var totalTime = tour.Logs.Sum(l => l.TimeTaken);

            AddKeyValuePairAsRow(statsTable, "Total Logs:", tour.Logs.Count.ToString(), font, boldFont);
            AddKeyValuePairAsRow(statsTable, "Average Rating:", $"{avgRating:F1}/5", font, boldFont);
            AddKeyValuePairAsRow(statsTable, "Average Difficulty:", $"{avgDifficulty:F1}/5", font, boldFont);
            AddKeyValuePairAsRow(statsTable, "Total Distance Logged:", $"{totalDistance:F2} km", font, boldFont);
            AddKeyValuePairAsRow(statsTable, "Total Time Logged:", $"{totalTime:F1} hours", font, boldFont);

            document.Add(statsTable);
        }
    }


    /// <summary>
    /// Adds the AI summary section to the PDF document for the given tour
    /// </summary>
    /// <param name="tour">Tour for which the AI summary should be added</param>
    /// <param name="boldFont">The font to use for bold text</param>
    /// <param name="document"> The PDF document to which the summary should be added</param>
    /// <param name="font">The font to use for normal text</param>
    private static void AddAiSummary(Tour tour, PdfFont boldFont, Document document, PdfFont font)
    {
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
    }


    /// <summary>
    /// Adds the logs section to the PDF document for the given tour
    /// </summary>
    /// <param name="tour">Tour for which the logs should be added</param>
    /// <param name="boldFont">The font to use for bold text</param>
    /// <param name="font">The font to use for normal text</param>
    /// <param name="document"> The PDF document to which the logs should be added</param>
    private void AddLogContainers(Tour tour, PdfFont boldFont, PdfFont font, Document document)
    {
        foreach (var log in tour.Logs.OrderByDescending(l => l.TimeStamp))
        {
            // Log container
            var logContainer = new Div()
                .SetMarginBottom(15)
                .SetPadding(10)
                .SetBorder(new SolidBorder(1));

            // Log header with timestamp
            var logHeader = new Paragraph($"Log Entry - {log.TimeStamp:dd/MM/yyyy HH:mm}")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetMarginBottom(8);
            logContainer.Add(logHeader);

            // Log details table
            var logTable = new Table(2, false);
            logTable.SetWidth(UnitValue.CreatePercentValue(100));

            AddKeyValuePairAsRow(logTable, "Rating:", $"{log.Rating:F1}/5", font, boldFont);
            AddKeyValuePairAsRow(logTable, "Difficulty:", $"{log.Difficulty}/5", font, boldFont);
            AddKeyValuePairAsRow(logTable, "Distance Traveled:", $"{log.DistanceTraveled:F2} km", font, boldFont);
            AddKeyValuePairAsRow(logTable, "Time Taken:", $"{log.TimeTaken:F1} hours", font, boldFont);

            if (!string.IsNullOrWhiteSpace(log.Comment))
                AddKeyValuePairAsRow(logTable, "Comment:", log.Comment, font, boldFont);

            logContainer.Add(logTable);
            document.Add(logContainer);
        }
    }


    /// <summary>
    /// Adds the tour details table to the PDF document
    /// </summary>
    /// <param name="tour">Tour for which the details should be added</param>
    /// <param name="font">The font to use for normal text</param>
    /// <param name="boldFont">The font to use for bold text</param>
    /// <param name="document">The PDF document to which the table should be added</param>
    private void AddTourDetails(Tour tour, PdfFont font, PdfFont boldFont, Document document)
    {
        var detailsTable = new Table(2);
        detailsTable.SetWidth(UnitValue.CreatePercentValue(100));

        AddKeyValuePairAsRow(detailsTable, "Description:", tour.TourDescription, font, boldFont);
        AddKeyValuePairAsRow(detailsTable, "Start Location:", tour.StartLocation, font, boldFont);
        AddKeyValuePairAsRow(detailsTable, "End Location:", tour.EndLocation, font, boldFont);
        AddKeyValuePairAsRow(detailsTable, "Means of transport:", tour.TransportationType.ToString(), font, boldFont);
        AddKeyValuePairAsRow(detailsTable, "Distance:", $"{tour.Distance:F2} km", font, boldFont);
        AddKeyValuePairAsRow(detailsTable, "Estimated Time:", $"{tour.EstimatedTime:F1} minutes", font, boldFont);

        AddKeyValuePairAsRow(detailsTable, "Popularity:", $"{tour.Popularity:F2} %", font, boldFont);
        AddKeyValuePairAsRow(detailsTable, "Child-Friendliness Rating:", $"{tour.ChildFriendlyRating:F2} %", font, boldFont);

        detailsTable.SetMarginBottom(10);
        document.Add(detailsTable);
    }


    /// <summary>
    /// Adds the route image to the PDF document for the given tour
    /// </summary>
    /// <param name="tour">Tour for which the route image should be added</param>
    /// <param name="document">The PDF document to which the image should be added</param>
    private async Task AddRouteImage(Tour tour, Document document)
    {
        try
        {
            var imagePath = await GetRouteImagePathAsync(tour);
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                var imageData = ImageDataFactory.Create(imagePath);
                var routeImage = new Image(imageData);
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
    }


    /// <summary>
    /// Generates the image of the route for the given tour and returns the file path to the image
    /// </summary>
    /// <param name="tour">The tour for which the route image should be generated</param>
    /// <returns>the file path to the generated route image</returns>
    private async Task<string> GetRouteImagePathAsync(Tour tour)
    {
        return await _mapService.CaptureMapImageAsync(tour);
    }
    
    
    // -------------------------
    // Tour Summary PDF Export
    // -------------------------
    
    /// <summary>
    /// Exports a summary of multiple tours as a PDF document
    /// </summary>
    /// <param name="tours">The list of tours to include in the summary</param>
    /// <param name="filePath">The file path where the PDF will be saved</param>
    /// <returns>True if the PDF was successfully created, false otherwise</returns>
    public async Task<bool> ExportToursAsPdfAsync(List<Tour> tours, string filePath)
    {
        try
        {
            var writer = new PdfWriter(filePath);
            var pdfDocument = new PdfDocument(writer);
            Document document = new(pdfDocument, PageSize.A4, false);

            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            document.SetMargins(20, 20, 20, 40);

            // Title
            var title = new Paragraph("Tours Summary Report")
                .SetFont(boldFont)
                .SetFontSize(24)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(title);

            // Generation date
            var dateGenerated = new Paragraph($"Generated on: {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetFont(font)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(25);
            document.Add(dateGenerated);

            // Overall statistics
            AddOverallStatistics(document, tours, font, boldFont);
            document.Add(new AreaBreak());

            // Tours summary table
            AddToursSummaryTable(document, tours, font, boldFont);
            document.Add(new AreaBreak());

            // Detailed tour breakdown
            AddDetailedTourBreakdown(document, tours, font, boldFont);

            document.Close();
            return File.Exists(filePath);
        }
        catch (Exception ex)
        {
            _logger.Error("Error generating tours summary PDF", ex);
            return false;
        }
    }


    /// <summary>
    /// Adds the overall statistics section to the PDF document
    /// </summary>
    /// <param name="document">The PDF document to add the statistics to</param>
    /// <param name="tours">The list of tours to calculate statistics from</param>
    /// <param name="font">The font to use for normal text</param>
    /// <param name="boldFont">The font to use for bold text</param>
    private void AddOverallStatistics(Document document, List<Tour> tours, PdfFont font, PdfFont boldFont)
    {
        var statsHeader = new Paragraph("Overall Statistics")
            .SetFont(boldFont)
            .SetFontSize(18)
            .SetMarginBottom(15);
        document.Add(statsHeader);

        // Calculate overall statistics
        int totalTours = tours.Count;
        var totalLogs = tours.Sum(t => t.Logs.Count);
        var toursWithLogs = tours.Where(t => t.Logs.Any()).ToList();
        var tourCountByTransportType = tours
            .GroupBy(t => t.TransportationType)
            .ToDictionary(g => g.Key, g => g.Count());

        // Overall statistics table
        var overallStatsTable = new Table(2);
        overallStatsTable.SetWidth(UnitValue.CreatePercentValue(100));
        overallStatsTable.SetMarginBottom(20);

        AddKeyValuePairAsRow(overallStatsTable, "Total Tours:", totalTours.ToString(), font, boldFont);
        AddKeyValuePairAsRow(overallStatsTable, "Tours with Logs:", $"{toursWithLogs.Count} ({(double)toursWithLogs.Count / totalTours * 100:F1}%)", font, boldFont);
        AddKeyValuePairAsRow(overallStatsTable, "Total Log Entries:", totalLogs.ToString(), font, boldFont);
        AddKeyValuePairAsRow(overallStatsTable, "Average Logs per Tour:", totalTours > 0 ? $"{(double)totalLogs / totalTours:F1}" : "0", font, boldFont);

        // Transport type breakdown
        foreach (var (transportType, count) in tourCountByTransportType)
        {
            double percentage = totalTours > 0 ? (double)count / totalTours * 100 : 0;
            AddKeyValuePairAsRow(overallStatsTable, $"{transportType} Tours:", $"{count} ({percentage:F1}%)", font, boldFont);
        }

        document.Add(overallStatsTable);
    }


    /// <summary>
    /// Adds the Tours Summary table to the PDF document
    /// </summary>
    /// <param name="document"> The PDF document to add the summary table to</param>
    /// <param name="tours">The list of tours to summarize</param>
    /// <param name="font">The font to use for normal text</param>
    /// <param name="boldFont"> The font to use for bold text</param>
    private void AddToursSummaryTable(Document document, List<Tour> tours, PdfFont font, PdfFont boldFont)
    {
        var summaryHeader = new Paragraph("Tours Summary")
            .SetFont(boldFont)
            .SetFontSize(18)
            .SetMarginBottom(15);
        document.Add(summaryHeader);

        // Create table with appropriate columns
        var summaryTable = new Table([3, 2, 2, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f]);
        summaryTable.SetWidth(UnitValue.CreatePercentValue(100));
        summaryTable.SetFontSize(10);

        // Header row
        var headerStyle = new Cell()
            .SetBackgroundColor(new DeviceRgb(230, 230, 230))
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFont(boldFont)
            .SetPadding(8);

        summaryTable.AddHeaderCell(headerStyle.Clone(false).Add(new Paragraph("Tour Name")));
        summaryTable.AddHeaderCell(headerStyle.Clone(false).Add(new Paragraph("Route")));
        summaryTable.AddHeaderCell(headerStyle.Clone(false).Add(new Paragraph("Transport")));
        summaryTable.AddHeaderCell(headerStyle.Clone(false).Add(new Paragraph("Logs")));
        summaryTable.AddHeaderCell(headerStyle.Clone(false).Add(new Paragraph("Avg. Rating")));
        summaryTable.AddHeaderCell(headerStyle.Clone(false).Add(new Paragraph("Avg. Time (h)")));
        summaryTable.AddHeaderCell(headerStyle.Clone(false).Add(new Paragraph("Avg. Distance (km)")));
        summaryTable.AddHeaderCell(headerStyle.Clone(false).Add(new Paragraph("Popularity")));

        // Data rows
        foreach (var tour in tours.OrderBy(t => t.TourName))
        {
            var logs = tour.Logs.ToList();
            var hasLogs = logs.Any();

            // Calculate averages
            var avgRating = hasLogs ? logs.Where(l => l.Rating > 0).Average(l => l.Rating) : 0;
            var avgTime = hasLogs ? logs.Average(l => l.TimeTaken) : 0;
            var avgDistance = hasLogs ? logs.Average(l => l.DistanceTraveled) : 0;

            var cellStyle = new Cell()
                .SetFont(font)
                .SetPadding(6)
                .SetTextAlignment(TextAlignment.CENTER);

            summaryTable.AddCell(cellStyle.Clone(false).SetTextAlignment(TextAlignment.LEFT).Add(new Paragraph(tour.TourName).SetFont(boldFont)));
            summaryTable.AddCell(cellStyle.Clone(false).SetTextAlignment(TextAlignment.LEFT).Add(new Paragraph($"{tour.StartLocation} -> {tour.EndLocation}")));
            summaryTable.AddCell(cellStyle.Clone(false).Add(new Paragraph(tour.TransportationType.ToString())));
            summaryTable.AddCell(cellStyle.Clone(false).Add(new Paragraph(logs.Count.ToString())));
            summaryTable.AddCell(cellStyle.Clone(false).Add(new Paragraph(avgRating > 0 ? $"{avgRating:F1}/5" : "N/A")));
            summaryTable.AddCell(cellStyle.Clone(false).Add(new Paragraph(hasLogs ? $"{avgTime:F1}" : "N/A")));
            summaryTable.AddCell(cellStyle.Clone(false).Add(new Paragraph(hasLogs ? $"{avgDistance:F1}" : "N/A")));
            summaryTable.AddCell(cellStyle.Clone(false).Add(new Paragraph($"{tour.Popularity:F1}%")));
        }

        document.Add(summaryTable);
    }

    /// <summary>
    /// Adds the detailed tour breakdown section to the PDF document
    /// </summary>
    /// <param name="document">The PDF document to add the breakdown to</param>
    /// <param name="tours">The list of tours to include in the breakdown</param>
    /// <param name="font">The font to use for normal text</param>
    /// <param name="boldFont"> The font to use for bold text</param>
    private void AddDetailedTourBreakdown(Document document, List<Tour> tours, PdfFont font, PdfFont boldFont)
    {
        var detailHeader = new Paragraph("Detailed Tour Breakdown")
            .SetFont(boldFont)
            .SetFontSize(18)
            .SetMarginTop(25)
            .SetMarginBottom(15);
        document.Add(detailHeader);

        foreach (var tour in tours.OrderBy(t => t.TourName))
        {
            // Tour container
            var tourContainer = new Div()
                .SetMarginBottom(20)
                .SetPadding(15)
                .SetBorder(new SolidBorder(1))
                .SetBorderRadius(new BorderRadius(5));

            // Tour header
            var tourHeader = new Paragraph(tour.TourName)
                .SetFont(boldFont)
                .SetFontSize(16)
                .SetMarginBottom(10);
            tourContainer.Add(tourHeader);

            // Basic Tour Info
            var tourInfoTable = new Table(2);
            tourInfoTable.SetWidth(UnitValue.CreatePercentValue(100));
            tourInfoTable.SetMarginBottom(10);

            AddKeyValuePairAsRow(tourInfoTable, "Route:", $"{tour.StartLocation} -> {tour.EndLocation}", font, boldFont);
            AddKeyValuePairAsRow(tourInfoTable, "Means of Transport:", tour.TransportationType.ToString(), font, boldFont);
            AddKeyValuePairAsRow(tourInfoTable, "Planned Distance:", $"{tour.Distance:F2} km", font, boldFont);
            AddKeyValuePairAsRow(tourInfoTable, "Estimated Time:", $"{tour.EstimatedTime:F1} minutes", font, boldFont);
            AddKeyValuePairAsRow(tourInfoTable, "Child-Friendliness:", $"{tour.ChildFriendlyRating:F1}%", font, boldFont);

            tourContainer.Add(tourInfoTable);

            // Log statistics
            var logs = tour.Logs.ToList();
            if (logs.Any())
            {
                var logStatsHeader = new Paragraph("Log Statistics")
                    .SetFont(boldFont)
                    .SetFontSize(14)
                    .SetMarginBottom(8);
                tourContainer.Add(logStatsHeader);

                var logStatsTable = new Table(2);
                logStatsTable.SetWidth(UnitValue.CreatePercentValue(100));

                var avgRating = logs.Where(l => l.Rating > 0).Average(l => l.Rating);
                var avgDifficulty = logs.Average(l => l.Difficulty);
                var avgTime = logs.Average(l => l.TimeTaken);
                var avgDistance = logs.Average(l => l.DistanceTraveled);
                var totalTime = logs.Sum(l => l.TimeTaken);
                var totalDistance = logs.Sum(l => l.DistanceTraveled);

                AddKeyValuePairAsRow(logStatsTable, "Total Logs:", logs.Count.ToString(), font, boldFont);
                AddKeyValuePairAsRow(logStatsTable, "Average Rating:", $"{avgRating:F1}/5", font, boldFont);
                AddKeyValuePairAsRow(logStatsTable, "Average Difficulty:", $"{avgDifficulty:F1}/5", font, boldFont);
                AddKeyValuePairAsRow(logStatsTable, "Average Time Taken:", $"{avgTime:F1} hours", font, boldFont);
                AddKeyValuePairAsRow(logStatsTable, "Average Distance Traveled:", $"{avgDistance:F1} km", font, boldFont);
                AddKeyValuePairAsRow(logStatsTable, "Total Time Logged:", $"{totalTime:F1} hours", font, boldFont);
                AddKeyValuePairAsRow(logStatsTable, "Total Distance Logged:", $"{totalDistance:F1} km", font, boldFont);

                // Efficiency metrics
                var timeEfficiency = tour.EstimatedTime > 0 ? avgTime * 60 / tour.EstimatedTime * 100 : 0; // Convert hours to minutes
                var distanceEfficiency = tour.Distance > 0 ? avgDistance / tour.Distance * 100 : 0;

                AddKeyValuePairAsRow(logStatsTable, "Time Efficiency:", timeEfficiency > 0 ? $"{timeEfficiency:F1}% of estimated" : "N/A", font, boldFont);
                AddKeyValuePairAsRow(logStatsTable, "Distance Efficiency:", distanceEfficiency > 0 ? $"{distanceEfficiency:F1}% of planned" : "N/A", font, boldFont);

                tourContainer.Add(logStatsTable);
            }
            else
            {
                var noLogsMessage = new Paragraph("No log entries available for this tour.")
                    .SetFont(font)
                    .SetFontSize(11)
                    .SetFontColor(ColorConstants.GRAY);
                tourContainer.Add(noLogsMessage);
            }
            
            document.Add(tourContainer);
        }
    }

    
    /// <summary>
    /// Adds a key-value pair as a row to the provided table
    /// </summary>
    /// <param name="table">The table to add the row to</param>
    /// <param name="key">The key for the row, typically a label</param>
    /// <param name="value">The value for the row, typically a description or data point</param>
    /// <param name="font">The font to use for the value</param>
    /// <param name="boldFont">The font to use for the label</param>
    private void AddKeyValuePairAsRow(Table table, string key, string value, PdfFont font, PdfFont boldFont)
    {
        var labelCell = new Cell()
            .Add(new Paragraph(key).SetFont(boldFont).SetFontSize(12))
            .SetPadding(5)
            .SetWidth(UnitValue.CreatePercentValue(30));

        var valueCell = new Cell()
            .Add(new Paragraph(value).SetFont(font).SetFontSize(12))
            .SetPadding(5)
            .SetWidth(UnitValue.CreatePercentValue(70));

        table.AddCell(labelCell);
        table.AddCell(valueCell);
    }
}