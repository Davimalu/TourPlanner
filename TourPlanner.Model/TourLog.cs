using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourPlanner.Model
{
    public class TourLog : INotifyPropertyChanged
    {
        private int _logId;
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogId
        {
            get => _logId;
            set
            {
                _logId = value;
                RaisePropertyChanged(nameof(LogId));
            }
        }

        private DateTime _timeStamp = DateTime.Now;
        public DateTime TimeStamp
        {
            get => _timeStamp;
            set
            {
                _timeStamp = value;
                RaisePropertyChanged(nameof(TimeStamp));
            }
        }


        private string _comment = string.Empty;
        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                RaisePropertyChanged(nameof(Comment));
            }
        }


        private int _difficulty = 3;
        public int Difficulty
        {
            get => _difficulty;
            set
            {
                _difficulty = value;
                RaisePropertyChanged(nameof(Difficulty));
            }
        }


        private float _distanceTraveled;
        public float DistanceTraveled
        {
            get => _distanceTraveled;
            set
            {
                _distanceTraveled = value;
                RaisePropertyChanged(nameof(DistanceTraveled));
            }
        }


        private float _timeTaken;
        public float TimeTaken
        {
            get => _timeTaken;
            set
            {
                _timeTaken = value;
                RaisePropertyChanged(nameof(TimeTaken));
            }
        }


        private float _rating = 0;
        public float Rating
        {
            get => _rating;
            set
            {
                _rating = value;
                RaisePropertyChanged(nameof(Rating));
            }
        }

        // Constructor
        public TourLog() { }

        // Copy constructor
        public TourLog(TourLog other)
        {
            LogId = other.LogId;
            TimeStamp = other.TimeStamp;
            Comment = other.Comment;
            Difficulty = other.Difficulty;
            DistanceTraveled = other.DistanceTraveled;
            TimeTaken = other.TimeTaken;
            Rating = other.Rating;
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}