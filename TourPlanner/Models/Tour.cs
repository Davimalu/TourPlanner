// Path: D:\Projects\Karl\TourPlanner-develop\TourPlanner.Model\Tour.cs
using System;
using TourPlanner.Models; // Optional, for other types if needed in the future

namespace TourPlanner.Model
{
    public class Tour
    {
        // Basic identification and description
        public Guid Id { get; set; } // A unique identifier for each tour
        public string Name { get; set; }
        public string Description { get; set; }

        // Origin information
        public string From { get; set; } // Textual representation of the start (e.g., "Vienna, Austria")
        public double FromLat { get; set; } // Latitude of the starting point
        public double FromLon { get; set; } // Longitude of the starting point

        // Destination information
        public string To { get; set; }   // Textual representation of the end (e.g., "Salzburg, Austria")
        public double ToLat { get; set; }   // Latitude of the ending point
        public double ToLon { get; set; }   // Longitude of the ending point

        // Route and travel information
        public double Distance { get; set; } // Estimated distance in kilometers or miles
        public TimeSpan EstimatedTime { get; set; } // Estimated time for the tour
        public string TransportType { get; set; } // e.g., "Car", "Bicycle", "Walking"

        // Additional details (optional, extend as needed)
        public string RouteInformation { get; set; } // Could be a polyline, or detailed instructions
        public string ImagePath { get; set; } // Path to an image representing the tour map or destination
        
        public int TourId { get; set; }
        public string TourName { get; set; }
        public List<TourLog> Logs { get; set; }

        // Default constructor (good practice)
        public Tour()
        {
            Id = Guid.NewGuid(); // Ensure every new tour gets a unique ID by default
            Name = string.Empty;
            Description = string.Empty;
            From = string.Empty;
            To = string.Empty;
            TransportType = string.Empty;
            RouteInformation = string.Empty;
            ImagePath = string.Empty;
        }
        
        public Tour(Tour other)
        {
            if (other == null) 
                throw new ArgumentNullException(nameof(other));

            Id = other.Id;
            TourName = other.TourName;
            // make a shallow copy of logs (or deep copy, as your app requires)
            Logs = new List<TourLog>(other.Logs);
            // copy any other properties...
        }

        // You might also consider adding a parameterized constructor for convenience:
        // public Tour(string name, string from, string to, /* other params */)
        // {
        //     Id = Guid.NewGuid();
        //     Name = name;
        //     From = from;
        //     To = to;
        //     // ... initialize other properties
        // }
    }
}