//using System;
//using TourPlanner.Enums;

//namespace TourPlanner.Models
//{
//    public class TourLog
//    {
//        public DateTime TimeStamp { get; set; }
//        public string Comment { get; set; } = string.Empty;
//        public Difficulty Difficulty { get; set; }
//        public float DistanceTraveled { get; set; }
//        public DateTime TimeTaken { get; set; }
//        public Rating Rating { get; set; }
//    }
//}

using System;
using System.ComponentModel;
using TourPlanner.Enums;

namespace TourPlanner.Models
{
    public class TourLog : INotifyPropertyChanged
    {
        private DateTime _timeStamp;
        private string _comment;
        private Difficulty _difficulty;
        private float _distanceTraveled;
        private DateTime _timeTaken;
        private Rating _rating;

        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set
            {
                _timeStamp = value;
                RaisePropertyChanged(nameof(TimeStamp));
            }
        }

        public string Comment
        {
            get { return _comment; }
            set
            {
                _comment = value;
                RaisePropertyChanged(nameof(Comment));
            }
        }

        public Difficulty Difficulty
        {
            get { return _difficulty; }
            set
            {
                _difficulty = value;
                RaisePropertyChanged(nameof(Difficulty));
            }
        }

        public float DistanceTraveled
        {
            get { return _distanceTraveled; }
            set
            {
                _distanceTraveled = value;
                RaisePropertyChanged(nameof(DistanceTraveled));
            }
        }

        public DateTime TimeTaken
        {
            get { return _timeTaken; }
            set
            {
                _timeTaken = value;
                RaisePropertyChanged(nameof(TimeTaken));
            }
        }

        public Rating Rating
        {
            get { return _rating; }
            set
            {
                _rating = value;
                RaisePropertyChanged(nameof(Rating));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}