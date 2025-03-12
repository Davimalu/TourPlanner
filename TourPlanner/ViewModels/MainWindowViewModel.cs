using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.Enums;
using TourPlanner.Models;

namespace TourPlanner.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private List<Tour> _tours;
        public List<Tour> Tours
        {
            get => _tours;
            set
            {
                _tours = value;
                RaisePropertyChanged(nameof(Tours));
            }
        }


        public MainWindowViewModel()
        {
            // Initialize dummy tours
            Tours = new List<Tour>
            {
                new Tour
                {
                    TourName = "Tour 1",
                    TourDescription = "Description 1",
                    StartLocation = "Start 1",
                    EndLocation = "End 1",
                    TransportationType = Transport.Bicycle,
                    Distance = 10,
                    EstimatedTime = new DateTime(2021, 1, 1),
                    RouteInformation = "Route 1"
                },
                new Tour
                {
                    TourName = "Tour 2",
                    TourDescription = "Description 2",
                    StartLocation = "Start 2",
                    EndLocation = "End 2",
                    TransportationType = Transport.Car,
                    Distance = 20,
                    EstimatedTime = new DateTime(2021, 2, 2),
                    RouteInformation = "Route 2"
                },
                new Tour
                {
                    TourName = "Tour 3",
                    TourDescription = "Description 3",
                    StartLocation = "Start 3",
                    EndLocation = "End 3",
                    TransportationType = Transport.Foot,
                    Distance = 30,
                    EstimatedTime = new DateTime(2021, 3, 3),
                    RouteInformation = "Route 3"
                }
            };
        }
    }
}
