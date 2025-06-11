using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TourPlanner.Enums;

namespace TourPlanner.RestServer.Models
{
    public class Tour
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TourId { get; set; }
        [Required]
        public string TourName { get; set; } = String.Empty;
        public string TourDescription { get; set; } = String.Empty;
        [Required]
        public string StartLocation { get; set; } = String.Empty;
        [Required]
        public string EndLocation { get; set; } = String.Empty;
        public Transport TransportationType { get; set; }
        public double Distance { get; set; }
        public float EstimatedTime { get; set; }
        
        // FIX: Add coordinate properties to be saved in the database
        public double StartLat { get; set; }
        public double StartLon { get; set; }
        public double EndLat { get; set; }
        public double EndLon { get; set; }

        public List<TourLog>? Logs { get; set; }
    }
}