using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TourPlanner.Commands;
using TourPlanner.Enums;
using TourPlanner.Logic;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Models;
using TourPlanner.Views;

namespace TourPlanner.ViewModels
{
    public class TourListViewModel : BaseViewModel
    {
        private readonly ISelectedTourService _selectedTourService;
        private readonly IWindowService _windowService = WindowService.Instance;


        private ObservableCollection<Tour>? _tours;
        public ObservableCollection<Tour>? Tours
        {
            get { return _tours; }
            set
            {
                _tours = value;
                RaisePropertyChanged(nameof(Tours));
            }
        }


        private string? _newTourName;
        public string? NewTourName
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
                _selectedTourService.SelectedTour = _selectedTour; // Update the selected tour in the service
            }
        }


        public TourListViewModel(ISelectedTourService selectedTourService)
        {
            _selectedTourService = selectedTourService;

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
                    RouteInformation = new BitmapImage(new Uri("https://www.niederoesterreich.at/images/d5tn5wton7g-/54c38e4576bea605b3d94ba94a49d669.jpg")),
                    Logs = new ObservableCollection<TourLog>() 
                    {
                        new TourLog
                        {
                            TimeStamp = DateTime.Now,
                            Comment = "Erster Log",
                            Difficulty = Difficulty.Easy,
                            DistanceTraveled = 10,
                            TimeTaken = 20,
                            Rating = Rating.Good
                        },
                        new TourLog
                        {
                            TimeStamp = DateTime.Now,
                            Comment = "Zweiter Log",
                            Difficulty = Difficulty.Medium,
                            DistanceTraveled = 15,
                            TimeTaken = 25,
                            Rating = Rating.Okay
                        }
                    }
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
                    RouteInformation = new BitmapImage(new Uri("https://vcdn.bergfex.at/images/resized/ff/9db2417c640525ff_7fef80f030ce570f@2x.jpg"))
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
                    RouteInformation = new BitmapImage(new Uri("https://vcdn.bergfex.at/images/resized/a0/c30bcdee9546ada0_c25240774a8f2541.jpg")),
                    Logs = new ObservableCollection<TourLog>()
                    {
                        new TourLog
                        {
                            TimeStamp = DateTime.Now,
                            Comment = "Erster Log",
                            Difficulty = Difficulty.Easy,
                            DistanceTraveled = 5,
                            TimeTaken = 16,
                            Rating = Rating.Good
                        },
                        new TourLog
                        {
                            TimeStamp = DateTime.Now,
                            Comment = "Zweiter Log",
                            Difficulty = Difficulty.Medium,
                            DistanceTraveled = 2,
                            TimeTaken = 28,
                            Rating = Rating.Okay
                        }
                    }
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
                    RouteInformation = new BitmapImage(new Uri("https://fahr-radwege.com/ThermenradwegEuroVeloTeil2.jpg"))
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
                    RouteInformation = new BitmapImage(new Uri("https://easycitypass.com/media/pages/blog/community-insider/wandern-vom-kahlenberg-uber-den-leopoldsberg-nach-nussdorf/b926bd4ae5-1686150598/Screenshot-2020-12-10-101455-1.png"))
                }
            };
        }


        public ICommand ExecuteAddNewTour => new RelayCommand(_ =>
        {
            Tours?.Add(new Tour() { TourName = NewTourName! });
            NewTourName = string.Empty;
        }, _ => !string.IsNullOrEmpty(NewTourName));


        public ICommand ExecuteDeleteTour => new RelayCommand(_ =>
        {
            if (SelectedTour != null)
            {
                Tours?.Remove(SelectedTour);
            }
            SelectedTour = null;
        }, _ => SelectedTour != null);


        public ICommand ExecuteEditTour => new RelayCommand(_ =>
        {
            _windowService.SpawnEditTourWindow(SelectedTour!);
        }, _ => SelectedTour != null);
    }
}
