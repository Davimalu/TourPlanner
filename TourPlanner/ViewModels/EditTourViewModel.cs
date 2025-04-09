using System.Windows.Input;
using System.Windows;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.DAL.ServiceAgents;
using TourPlanner.Enums;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Models;

namespace TourPlanner.ViewModels
{
    class EditTourViewModel : BaseViewModel
    {
        private readonly ITourService _tourService = new TourService();
        private readonly ILoggerWrapper _logger;

        // The original Tour before it was edited
        private Tour _originalTour;


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
            EditableTour = new Tour(_originalTour); // Create a copy of the Tour to edit

            _logger = LoggerFactory.GetLogger<EditTourViewModel>();

            // Initialize enums
            Transports = new List<Transport> { Transport.Bicycle, Transport.Car, Transport.Foot, Transport.Motorcycle };
        }


        public ICommand ExecuteSave => new RelayCommandAsync(async _ =>
        {
            // Check if the Tour already exists (i.e. are we updating an existing Tour or creating a new one?)

            // Tour doesn't exist -> create it
            if (_originalTour.TourId == -1)
            {
                Tour? newTour = await _tourService.CreateTourAsync(EditableTour);

                if (newTour == null)
                {
                    _logger.Error($"Failed to create Tour: {EditableTour.TourName}");
                }
            }
            // Tour already exists -> update it
            else
            {
                Tour? updatedTour = await _tourService.UpdateTourAsync(EditableTour);

                if (updatedTour == null)
                {
                    _logger.Error($"Failed to update Tour with ID {EditableTour.TourId}: {EditableTour.TourName}");
                }
            }

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
