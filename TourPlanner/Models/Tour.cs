using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using TourPlanner.Enums;
using TourPlanner.Models;

namespace TourPlanner.Model
{
    public class Tour : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _tourId;
        private string _tourName = string.Empty;
        private string _tourDescription = string.Empty;
        private string _startLocation = string.Empty;
        private string _endLocation = string.Empty;
        private Transport _transportationType = Transport.Car;
        private double _distance;
        private float _estimatedTime;
        private double _startLat, _startLon, _endLat, _endLon;
        
        [JsonIgnore]
        public Guid Id { get; set; }
        
        [JsonPropertyName("tourId")]
        public int TourId { get => _tourId; set { _tourId = value; OnPropertyChanged(); } }

        [JsonPropertyName("tourName")]
        public string TourName { get => _tourName; set { _tourName = value; OnPropertyChanged(); } }

        [JsonPropertyName("tourDescription")]
        public string TourDescription { get => _tourDescription; set { _tourDescription = value; OnPropertyChanged(); } }

        [JsonPropertyName("startLocation")]
        public string StartLocation { get => _startLocation; set { _startLocation = value; OnPropertyChanged(); } }

        [JsonPropertyName("endLocation")]
        public string EndLocation { get => _endLocation; set { _endLocation = value; OnPropertyChanged(); } }
        
        [JsonPropertyName("transportationType")]
        public Transport TransportationType { get => _transportationType; set { _transportationType = value; OnPropertyChanged(); } }

        [JsonPropertyName("distance")]
        public double Distance { get => _distance; set { _distance = value; OnPropertyChanged(); } }
        
        [JsonPropertyName("estimatedTime")]
        public float EstimatedTime { get => _estimatedTime; set { _estimatedTime = value; OnPropertyChanged(); } }
        
        // FIX: Add coordinate properties that are sent to/from the server
        [JsonPropertyName("startLat")]
        public double StartLat { get => _startLat; set { _startLat = value; OnPropertyChanged(); } }
        [JsonPropertyName("startLon")]
        public double StartLon { get => _startLon; set { _startLon = value; OnPropertyChanged(); } }
        [JsonPropertyName("endLat")]
        public double EndLat { get => _endLat; set { _endLat = value; OnPropertyChanged(); } }
        [JsonPropertyName("endLon")]
        public double EndLon { get => _endLon; set { _endLon = value; OnPropertyChanged(); } }

        [JsonPropertyName("logs")]
        public List<TourLog> Logs { get; set; }

        [JsonIgnore] public string RouteInformation { get; set; } = string.Empty;
        [JsonIgnore] public string ImagePath { get; set; } = string.Empty;
        
        public Tour()
        {
            Id = Guid.NewGuid();
            Logs = new List<TourLog>();
        }
        
        public Tour(Tour other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            
            Id = other.Id;
            TourId = other.TourId;
            TourName = other.TourName;
            TourDescription = other.TourDescription;
            StartLocation = other.StartLocation;
            EndLocation = other.EndLocation;
            TransportationType = other.TransportationType;
            Distance = other.Distance;
            EstimatedTime = other.EstimatedTime;
            StartLat = other.StartLat; StartLon = other.StartLon;
            EndLat = other.EndLat; EndLon = other.EndLon;
            Logs = new List<TourLog>(other.Logs ?? new List<TourLog>());
            RouteInformation = other.RouteInformation;
            ImagePath = other.ImagePath;
        }
    }
}