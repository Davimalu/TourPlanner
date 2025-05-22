using System;
using TourPlanner.Model;

namespace TourPlanner.Logic.Interfaces
{
    public interface ISelectedTourService
    {
        /// <summary>
        /// Event that is raised when the selected tour changes.
        /// Provides the newly selected tour (or null if no tour is selected).
        /// </summary>
        event Action<Tour?> SelectedTourChanged;
        
        Tour? SelectedTour { get; set; }

        /// <summary>
        /// Gets the currently selected tour.
        /// </summary>
        Tour? CurrentSelectedTour { get; }

        /// <summary>
        /// Method to update the currently selected tour.
        /// Implementations should raise the SelectedTourChanged event.
        /// </summary>
        /// <param name="tour">The tour to select, or null to deselect.</param>
        void SelectTour(Tour? tour);
    }
}