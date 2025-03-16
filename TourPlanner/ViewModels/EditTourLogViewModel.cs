using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.Enums;
using TourPlanner.Models;
using TourPlanner.Views;

namespace TourPlanner.ViewModels
{
    public class EditTourLogViewModel : BaseViewModel
    {
        private TourLog _tourLog;
        public TourLog TourLog
        {
            get { return _tourLog; }
            set
            {
                _tourLog = value;
                RaisePropertyChanged(nameof(TourLog));
            }
        }

        public string Comment
        {
            get { return TourLog?.Comment ?? string.Empty; }
            set
            {
                TourLog.Comment = value;
                RaisePropertyChanged(nameof(Comment));
            }
        }

        public Difficulty SelectedDifficulty
        {
            get { return TourLog?.Difficulty ?? Difficulty.Easy; } // Default to Easy if TourLog is null
            set
            {
                if (TourLog != null)
                {
                    TourLog.Difficulty = value;
                    RaisePropertyChanged(nameof(SelectedDifficulty));
                }
            }
        }

        public float DistanceTraveled
        {
            get { return TourLog?.DistanceTraveled ?? 0; } // Default to 0 if TourLog is null
            set
            {
                if (TourLog != null)
                {
                    TourLog.DistanceTraveled = value;
                    RaisePropertyChanged(nameof(DistanceTraveled));
                }
            }
        }

        public DateTime TimeTaken
        {
            get { return TourLog?.TimeTaken ?? DateTime.MinValue; } // Default to DateTime.MinValue if TourLog is null
            set
            {
                if (TourLog != null)
                {
                    TourLog.TimeTaken = value;
                    RaisePropertyChanged(nameof(TimeTaken));
                }
            }
        }

        public Rating SelectedRating
        {
            get { return TourLog?.Rating ?? Rating.Bad; } // Default to Bad if TourLog is null
            set
            {
                if (TourLog != null)
                {
                    TourLog.Rating = value;
                    RaisePropertyChanged(nameof(SelectedRating));
                }
            }
        }

        public List<Difficulty> Difficulties { get; set; }
        public List<Rating> Ratings { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private readonly Action<TourLog> _saveCallback;

        public EditTourLogViewModel(TourLog tourLog, Action<TourLog> saveCallback)
        {
            TourLog = tourLog ?? new TourLog();
            _saveCallback = saveCallback;

            // Initialize enums
            Difficulties = new List<Difficulty> { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard };
            Ratings = new List<Rating> { Rating.Bad, Rating.Okay, Rating.Good, Rating.Great, Rating.Amazing };

            // Initialize commands
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private void Save()
        {
            // Trigger the SaveChanges event
            _saveCallback?.Invoke(TourLog);


            // Close the window
            CloseWindow();
        }

        private void Cancel()
        {
            // Close the window without saving changes
            CloseWindow();
        }

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