using System.IO;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;

namespace TourPlanner.Logic;

public class PdfService : IPdfService
{
    public Task<bool> ExportTourAsPdfAsync(Tour tour, string filePath)
    {
        PdfWriter writer = new PdfWriter(filePath);
        PdfDocument pdfDocument = new PdfDocument(writer);
        Document document = new(pdfDocument, PageSize.A4, false);

        var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        document.SetMargins(20, 20, 20, 40);

        var pTitle = new Paragraph($"{tour.TourName}")
            .SetFont(font)
            .SetFontSize(24)
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
        
        document.Add(pTitle);

        document.Close();
        
        return Task.FromResult(File.Exists(filePath));
    }
}