using System;
using System.ComponentModel;
using TourPlanner.Enums;

namespace TourPlanner.Models
{
    public class TourLog : INotifyPropertyChanged
    {
        // TourLog needs to implement INotifyPropertyChanged to notify the UI when a property changes
        // This isn't ideal but according to StackOverflow it's acceptable
        // https://stackoverflow.com/questions/6922130/in-mvvm-model-should-the-model-implement-inotifypropertychanged-interface


        private int _logId;
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


        private Difficulty _difficulty = Difficulty.Medium;
        public Difficulty Difficulty
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


        private Rating _rating = Rating.Good;
        public Rating Rating
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
        public TourLog(TourLog tourLog)
        {
            LogId = tourLog.LogId;
            TimeStamp = tourLog.TimeStamp;
            Comment = tourLog.Comment;
            Difficulty = tourLog.Difficulty;
            DistanceTraveled = tourLog.DistanceTraveled;
            TimeTaken = tourLog.TimeTaken;
            Rating = tourLog.Rating;
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}