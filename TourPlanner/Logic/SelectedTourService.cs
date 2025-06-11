using System;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;

namespace TourPlanner.Logic
{
    public class SelectedTourService : ISelectedTourService
    {
        private Tour? _currentSelectedTour;

        public event Action<Tour?> SelectedTourChanged;
        
        // ← This is the missing interface member:
        public Tour? SelectedTour {
            get => _currentSelectedTour;
            set => SelectTour(value);
        }

        public Tour? CurrentSelectedTour => _currentSelectedTour;

        public void SelectTour(Tour? tour)
        {
            // Check if the selection is actually changing to avoid redundant events
            // This basic check compares references. For more complex scenarios,
            // you might compare IDs or use IEquatable<Tour>.
            if (!ReferenceEquals(_currentSelectedTour, tour))
            {
                _currentSelectedTour = tour;
                System.Diagnostics.Debug.WriteLine($"SelectedTourService: Tour selection changed to -> {(_currentSelectedTour?.TourName ?? "None")}");
                SelectedTourChanged?.Invoke(_currentSelectedTour);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"SelectedTourService: Tour re-selected or selection unchanged -> {(_currentSelectedTour?.TourName ?? "None")}");
                // Optionally, you might still want to invoke if properties of the same tour instance changed
                // and subscribers need to react. For a simple selection change, this is usually not needed.
                // SelectedTourChanged?.Invoke(_currentSelectedTour);
            }
        }
    }
}