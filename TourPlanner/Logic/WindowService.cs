using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Models;
using TourPlanner.ViewModels;
using TourPlanner.Views;

namespace TourPlanner.Logic
{
    internal class WindowService : IWindowService
    {
        #region Singleton
        private static WindowService? _instance;
        public static WindowService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WindowService();
                }
                return _instance;
            }
        }
        #endregion

        public void SpawnEditTourWindow(Tour selectedTour)
        {
            var editWindow = new EditTourWindow()
            {
                DataContext = new EditTourViewModel(selectedTour)
            };

            editWindow.ShowDialog();
        }


        public void SpawnEditTourLogWindow(TourLog selectedTourLog)
        {
            var editWindow = new EditTourLogWindow
            {
                DataContext = new EditTourLogViewModel(selectedTourLog)
            };

            editWindow.ShowDialog();
        }
    }
}
