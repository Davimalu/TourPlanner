using System;
using TourPlanner.Model;

namespace TourPlanner.Logic.Interfaces
{
    public interface ISelectedTourService
    {
        Tour? SelectedTour { get; set; }
        /// <summary>
        /// Event that is raised when the selected tour changes.
        /// Provides the newly selected tour (or null if no tour is selected).
        /// </summary>
        event Action<Tour?> SelectedTourChanged;
    }
}