using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TourPlanner.Model.Enums;
using TourPlanner.Model.Structs;

namespace TourPlanner.Model
{
    public class Tour : INotifyPropertyChanged
    {
        /* One could argue that it is Bad Practice to implement INotifyPropertyChanged in a model class since it is strictly speaking a concern of the ViewModel,
         BUT not doing so would require a lot of boilerplate code in the ViewModel. (It basically has to wrap all the properties of the model in its own properties.)
         This leads to quite a lot of code duplication, makes the ViewModel harder to read and maintain, and adds unnecessary complexity.
         Thus, we have decided to implement INotifyPropertyChanged in the model class - since it's an abstraction layer not directly tied to the UI (could theoretically be used by other frameworks than WPF), we consider it acceptable.
         
         Also see this discussion on StackOverflow: https://stackoverflow.com/questions/6922130/in-mvvm-model-should-the-model-implement-inotifypropertychanged-interface
         */
        
        // TODO: Determine why the JSONPropertyName attributes are necessary here
        
        private int _tourId;
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("tourId")]
        public int TourId
        {
            get => _tourId;
            set
            {
                _tourId = value;
                OnPropertyChanged(nameof(TourId));
            }
        }
        
        private string _tourName = string.Empty;
        [JsonPropertyName("tourName")]
        [Required]
        public string TourName
        {
            get => _tourName;
            set
            {
                _tourName = value;
                OnPropertyChanged(nameof(TourName));
            }
        }
        
        private string _tourDescription = string.Empty;
        [JsonPropertyName("tourDescription")]
        public string TourDescription
        {
            get => _tourDescription;
            set
            {
                _tourDescription = value;
                OnPropertyChanged(nameof(TourDescription));
            }
        }
        
        private string _startLocation = string.Empty;
        [JsonPropertyName("startLocation")]
        [Required]
        public string StartLocation
        {
            get => _startLocation;
            set
            {
                _startLocation = value;
                OnPropertyChanged(nameof(StartLocation));
            }
        }
        
        private string _endLocation = string.Empty;
        [JsonPropertyName("endLocation")]
        [Required]
        public string EndLocation
        {
            get => _endLocation;
            set {
                _endLocation = value;
                OnPropertyChanged(nameof(EndLocation));
            }
        }
        
        private Transport _transportationType = Transport.Car;
        [JsonPropertyName("transportationType")]
        public Transport TransportationType
        {
            get => _transportationType;
            set
            {
                _transportationType = value;
                OnPropertyChanged(nameof(TransportationType));
            }
        }
        
        private double _distance;
        [JsonPropertyName("distance")]
        public double Distance
        {
            get => _distance;
            set
            {
                _distance = value;
                OnPropertyChanged(nameof(Distance));
            }
        }
        
        private float _estimatedTime;
        [JsonPropertyName("estimatedTime")]
        public float EstimatedTime
        {
            get => _estimatedTime;
            set
            {
                _estimatedTime = value;
                OnPropertyChanged(nameof(EstimatedTime));
            }
        }

        private GeoCoordinate? _startCoordinates;
        [JsonPropertyName("startCoordinates")]
        public GeoCoordinate? StartCoordinates
        {
            get => _startCoordinates;
            set
            {
                _startCoordinates = value;
                OnPropertyChanged(nameof(StartCoordinates));
            }
        }
        
        private GeoCoordinate? _endCoordinates;
        [JsonPropertyName("endCoordinates")]
        public GeoCoordinate? EndCoordinates
        {
            get => _endCoordinates;
            set
            {
                _endCoordinates = value;
                OnPropertyChanged(nameof(EndCoordinates));
            }
        }
        
        // Computed attributes
        private float _popularity;
        [JsonIgnore]
        public float Popularity
        {
            get => _popularity;
            set
            {
                _popularity = value;
                OnPropertyChanged(nameof(Popularity));
            }
        }
        
        private float _childFriendlyRating;
        [JsonIgnore]
        public float ChildFriendlyRating
        {
            get => _childFriendlyRating;
            set
            {
                _childFriendlyRating = value;
                OnPropertyChanged(nameof(ChildFriendlyRating));
            }
        }
        
        private string _aiSummary = string.Empty;
        [JsonIgnore]
        public string AiSummary
        {
            get => _aiSummary;
            set
            {
                _aiSummary = value;
                OnPropertyChanged(nameof(AiSummary));
            }
        }
        
        // Tour logs
        private ObservableCollection<TourLog> _logs = new ObservableCollection<TourLog>();
        [JsonPropertyName("logs")]
        public ObservableCollection<TourLog> Logs
        {
            get => _logs;
            set 
            {
                _logs = value;
                OnPropertyChanged(nameof(Logs));
            }
        }
        
        
        // Constructor
        public Tour() { }
        
        // Copy constructor
        public Tour(Tour other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            
            TourId = other.TourId;
            TourName = other.TourName;
            TourDescription = other.TourDescription;
            StartLocation = other.StartLocation;
            EndLocation = other.EndLocation;
            TransportationType = other.TransportationType;
            Distance = other.Distance;
            EstimatedTime = other.EstimatedTime;
            StartCoordinates = other.StartCoordinates;
            EndCoordinates = other.EndCoordinates;
            Logs = new ObservableCollection<TourLog>(other.Logs);
        }
        
        
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}