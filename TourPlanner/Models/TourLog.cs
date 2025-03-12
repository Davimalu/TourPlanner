using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourPlanner.Enums;

namespace TourPlanner.Models
{
    public class TourLog
    {
        public DateTime TimeStamp { get; set; }
        public string Comment { get; set; } = String.Empty;
        public Difficulty Difficulty { get; set; }
        public float DistanceTraveled { get; set; }
        public DateTime TimeTaken { get; set; }
        public Rating Rating { get; set; }
    }
}
