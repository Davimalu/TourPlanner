using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.Enums;
using TourPlanner.Models;

namespace TourPlanner.ViewModels
{
    public class TourListViewModel : BaseViewModel
    {
        private ObservableCollection<Tour> _tours;
        public ObservableCollection<Tour> Tours
        {
            get { return _tours; }
            set
            {
                _tours = value;
                RaisePropertyChanged(nameof(Tours));
            }
        }


        private string _newTourName;
        public string NewTourName
        {
            get { return _newTourName; }
            set
            {
                _newTourName = value;
                RaisePropertyChanged(nameof(NewTourName));
            }
        }


        private Tour? _selectedTour;
        public Tour? SelectedTour
        {
            get { return _selectedTour; }
            set
            {
                _selectedTour = value;
                RaisePropertyChanged(nameof(SelectedTour));
                SelectedTourChanged?.Invoke(SelectedTour);
            }
        }

        public event Action<Tour?>? SelectedTourChanged;



        public TourListViewModel()
        {
            // TODO: Get this information from the database instead

            // Initialize dummy tours
            Tours = new ObservableCollection<Tour>
            {
                new Tour
                {
                    TourName = "Wienerwaldrundweg",
                    TourDescription = "Eine malerische Fahrradtour durch den Wienerwald mit herrlichem Ausblick auf Wien.",
                    StartLocation = "Hütteldorf, Wien",
                    EndLocation = "Hütteldorf, Wien",
                    TransportationType = Transport.Bicycle,
                    Distance = 35,
                    EstimatedTime = 110,
                    RouteInformation = "Von Hütteldorf über Purkersdorf, Mauerbach und zurück nach Hütteldorf."
                },
                new Tour
                {
                    TourName = "Weinviertel Panoramatour",
                    TourDescription = "Eine entspannte Autofahrt durch das Weinviertel mit Stopps bei Weingütern.",
                    StartLocation = "Korneuburg",
                    EndLocation = "Retz",
                    TransportationType = Transport.Car,
                    Distance = 75,
                    EstimatedTime = 200,
                    RouteInformation = "Von Korneuburg über Stockerau, Hollabrunn nach Retz mit schönen Aussichten auf die Weinberge."
                },
                new Tour
                {
                    TourName = "Donauinsel Spaziergang",
                    TourDescription = "Ein gemütlicher Spaziergang entlang der Donauinsel mit vielen Rastmöglichkeiten.",
                    StartLocation = "Reichsbrücke, Wien",
                    EndLocation = "Floridsdorfer Brücke, Wien",
                    TransportationType = Transport.Foot,
                    Distance = 7,
                    EstimatedTime = 110,
                    RouteInformation = "Entlang der Donauinsel von der Reichsbrücke zur Floridsdorfer Brücke mit schöner Natur und Ausblicken."
                },
                new Tour
                {
                    TourName = "Thermenradweg",
                    TourDescription = "Eine schöne Radtour entlang des Thermenradwegs von Wien nach Baden.",
                    StartLocation = "Wienerberg, Wien",
                    EndLocation = "Baden bei Wien",
                    TransportationType = Transport.Bicycle,
                    Distance = 28,
                    EstimatedTime = 70,
                    RouteInformation = "Von Wienerberg entlang des Wiener Neustädter Kanals nach Baden mit vielen Rastmöglichkeiten."
                },
                new Tour
                {
                    TourName = "Leopoldsberg-Wanderung",
                    TourDescription = "Eine anspruchsvolle Wanderung mit atemberaubender Aussicht über Wien.",
                    StartLocation = "Kahlenbergerdorf, Wien",
                    EndLocation = "Leopoldsberg",
                    TransportationType = Transport.Foot,
                    Distance = 5,
                    EstimatedTime = 60,
                    RouteInformation = "Von Kahlenbergerdorf den steilen Wanderweg hoch zum Leopoldsberg mit fantastischer Aussicht."
                }
            };
        }


        public ICommand ExecuteAddNewTour => new RelayCommand(_ =>
        {
            Tours.Add(new Tour() { TourName = NewTourName });
            NewTourName = string.Empty;
        }, _ => !string.IsNullOrEmpty(NewTourName));


        public ICommand ExecuteDeleteTour => new RelayCommand(_ =>
        {
            if (SelectedTour != null)
            {
                Tours.Remove(SelectedTour);
            }
            SelectedTour = null;
        }, _ => SelectedTour != null);
    }
}
