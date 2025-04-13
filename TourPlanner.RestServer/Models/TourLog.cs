using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TourPlanner.Enums;

namespace TourPlanner.RestServer.Models
{
    public class TourLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogId { get; set; }
        [Required]
        public DateTime TimeStamp { get; set; }
        public string Comment { get; set; } = String.Empty;
        public int Difficulty { get; set; }
        public float DistanceTraveled { get; set; }
        public float TimeTaken { get; set; }
        public float Rating { get; set; }
    }
}