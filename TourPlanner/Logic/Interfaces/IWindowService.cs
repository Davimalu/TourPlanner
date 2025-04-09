using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourPlanner.Models;

namespace TourPlanner.Logic.Interfaces
{
    internal interface IWindowService
    {
        public void SpawnEditTourWindow(Tour selectedTour);
        public void SpawnEditTourLogWindow (Tour selectedTour, TourLog selectedTourLog);
    }
}
