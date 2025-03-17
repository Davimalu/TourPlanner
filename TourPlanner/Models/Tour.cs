using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using TourPlanner.Enums;

namespace TourPlanner.Models
{
    public class Tour : INotifyPropertyChanged
    {
        private string _tourName = string.Empty;
        public string TourName
        {
            get => _tourName;
            set
            {
                _tourName = value;
                RaisePropertyChanged(nameof(TourName));
            }
        }

        private string _tourDescription = string.Empty;
        public string TourDescription
        {
            get => _tourDescription;
            set
            {
                _tourDescription = value;
                RaisePropertyChanged(nameof(TourDescription));
            }
        }

        private string _startLocation = string.Empty;
        public string StartLocation
        {
            get => _startLocation;
            set
            {
                _startLocation = value;
                RaisePropertyChanged(nameof(StartLocation));
            }
        }

        private string _endLocation = string.Empty;
        public string EndLocation
        {
            get => _endLocation;
            set
            {
                _endLocation = value;
                RaisePropertyChanged(nameof(EndLocation));
            }
        }

        private Transport _transportationType;
        public Transport TransportationType
        {
            get => _transportationType;
            set
            {
                _transportationType = value;
                RaisePropertyChanged(nameof(TransportationType));
            }
        }

        private float _distance;
        public float Distance
        {
            get => _distance;
            set
            {
                _distance = value;
                RaisePropertyChanged(nameof(Distance));
            }
        }

        private float _estimatedTime;
        public float EstimatedTime
        {
            get => _estimatedTime;
            set
            {
                _estimatedTime = value;
                RaisePropertyChanged(nameof(EstimatedTime));
            }
        }

        private BitmapImage _routeInformation;
        public BitmapImage RouteInformation
        {
            get => _routeInformation;
            set
            {
                _routeInformation = value;
                RaisePropertyChanged(nameof(RouteInformation));
            }
        }

        private ObservableCollection<TourLog> _logs = new ObservableCollection<TourLog>();
        public ObservableCollection<TourLog> Logs
        {
            get => _logs;
            set
            {
                _logs = value;
                RaisePropertyChanged(nameof(Logs));
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
