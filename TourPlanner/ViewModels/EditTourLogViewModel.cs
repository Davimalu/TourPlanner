using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.Enums;
using TourPlanner.Models;

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
            get { return TourLog.Comment; }
            set
            {
                TourLog.Comment = value;
                RaisePropertyChanged(nameof(Comment));
            }
        }

        public Difficulty SelectedDifficulty
        {
            get { return TourLog.Difficulty; }
            set
            {
                TourLog.Difficulty = value;
                RaisePropertyChanged(nameof(SelectedDifficulty));
            }
        }

        public float DistanceTraveled
        {
            get { return TourLog.DistanceTraveled; }
            set
            {
                TourLog.DistanceTraveled = value;
                RaisePropertyChanged(nameof(DistanceTraveled));
            }
        }

        public DateTime TimeTaken
        {
            get { return TourLog.TimeTaken; }
            set
            {
                TourLog.TimeTaken = value;
                RaisePropertyChanged(nameof(TimeTaken));
            }
        }

        public Rating SelectedRating
        {
            get { return TourLog.Rating; }
            set
            {
                TourLog.Rating = value;
                RaisePropertyChanged(nameof(SelectedRating));
            }
        }

        public List<Difficulty> Difficulties { get; set; }
        public List<Rating> Ratings { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public EditTourLogViewModel()
        {
            // Initialize enums
            Difficulties = new List<Difficulty> { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard };
            Ratings = new List<Rating> { Rating.Bad, Rating.Okay, Rating.Good, Rating.Great, Rating.Amazing };

            // Initialize commands
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private void Save()
        {
            // Close the window and save changes
            // (The main ViewModel will handle updating the log list)
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