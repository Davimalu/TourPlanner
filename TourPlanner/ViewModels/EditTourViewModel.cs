using System.Windows.Input;
using System.Windows;
using TourPlanner.Commands;
using TourPlanner.Enums;
using TourPlanner.Models;

namespace TourPlanner.ViewModels
{
    class EditTourViewModel : BaseViewModel
    {
        // The original Tour before it was edited
        private readonly Tour _originalTour;


        // The copy that will be edited
        private Tour _editableTour = null!;
        public Tour EditableTour
        {
            get { return _editableTour; }
            set
            {
                _editableTour = value;
                RaisePropertyChanged(nameof(EditableTour));
            }
        }


        public string TourName
        {
            get { return EditableTour.TourName; }
            set
            {
                EditableTour.TourName = value;
                RaisePropertyChanged(nameof(TourName));
            }
        }

        public string TourDescription
        {
            get { return EditableTour.TourDescription; }
            set
            {
                EditableTour.TourDescription = value;
                RaisePropertyChanged(nameof(TourDescription));
            }
        }

        public string StartLocation
        {
            get { return EditableTour.StartLocation; }
            set
            {
                EditableTour.StartLocation = value;
                RaisePropertyChanged(nameof(StartLocation));
            }
        }

        public string EndLocation
        {
            get { return EditableTour.EndLocation; }
            set
            {
                EditableTour.EndLocation = value;
                RaisePropertyChanged(nameof(EndLocation));
            }
        }

        public Transport SelectedTransport
        {
            get { return EditableTour.TransportationType; }
            set
            {
                EditableTour.TransportationType = value;
                RaisePropertyChanged(nameof(SelectedTransport));
            }
        }

        public float TourDistance
        {
            get { return EditableTour.Distance; }
            set
            {
                EditableTour.Distance = value;
                RaisePropertyChanged(nameof(TourDistance));
            }
        }

        public float EstimatedTime
        {
            get { return EditableTour.EstimatedTime; }
            set
            {
                EditableTour.EstimatedTime = value;
                RaisePropertyChanged(nameof(EstimatedTime));
            }
        }


        public List<Transport> Transports { get; set; }

        public EditTourViewModel(Tour selectedTour)
        {
            _originalTour = selectedTour; // Store the original Tour
            EditableTour = new Tour() // Create a copy of the Tour to edit
            {
                TourName = selectedTour.TourName,
                TourDescription = selectedTour.TourDescription,
                StartLocation = selectedTour.StartLocation,
                EndLocation = selectedTour.EndLocation,
                TransportationType = selectedTour.TransportationType,
                Distance = selectedTour.Distance,
                EstimatedTime = selectedTour.EstimatedTime
            };


            // Initialize enums
            Transports = new List<Transport> { Transport.Bicycle, Transport.Car, Transport.Foot, Transport.Motorcycle };
        }


        public ICommand ExecuteSave => new RelayCommand(_ =>
        {
            // Copy the changes from the editable copy to the original
            _originalTour.TourName = EditableTour.TourName;
            _originalTour.TourDescription = EditableTour.TourDescription;
            _originalTour.StartLocation = EditableTour.StartLocation;
            _originalTour.EndLocation = EditableTour.EndLocation;
            _originalTour.TransportationType = EditableTour.TransportationType;
            _originalTour.Distance = EditableTour.Distance;
            _originalTour.EstimatedTime = EditableTour.EstimatedTime;

            // Close the window
            CloseWindow();
        });


        public ICommand ExecuteCancel => new RelayCommand(_ =>
        {
            // Close the window, discarding changes
            CloseWindow();
        });


        private void CloseWindow()
        {
            // Close the window
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}
