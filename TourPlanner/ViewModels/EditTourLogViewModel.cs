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
        private TourLog _selectedTourLog;
        public TourLog SelectedTourLog
        {
            get { return _selectedTourLog; }
            set
            {
                _selectedTourLog = value;
                RaisePropertyChanged(nameof(SelectedTourLog));
            }
        }


        public DateTime TimeStamp
        {
            get { return _selectedTourLog.TimeStamp; }
            set
            {
                _selectedTourLog.TimeStamp = value;
                RaisePropertyChanged(nameof(TimeStamp));
            }
        }


        public string Comment
        {
            get { return SelectedTourLog.Comment; }
            set
            {
                SelectedTourLog.Comment = value;
                RaisePropertyChanged(nameof(Comment));
            }
        }


        public Difficulty SelectedDifficulty
        {
            get { return SelectedTourLog.Difficulty; } 
            set
            {
                SelectedTourLog.Difficulty = value;
                RaisePropertyChanged(nameof(SelectedDifficulty));
            }
        }


        public float DistanceTraveled
        {
            get { return SelectedTourLog.DistanceTraveled; }
            set
            {
                SelectedTourLog.DistanceTraveled = value;
                RaisePropertyChanged(nameof(DistanceTraveled));
            }
        }

        public float TimeTaken
        {
            get { return SelectedTourLog.TimeTaken; }
            set
            {
                SelectedTourLog.TimeTaken = value;
                RaisePropertyChanged(nameof(TimeTaken));
            }
        }


        public Rating SelectedRating
        {
            get { return SelectedTourLog.Rating; }
            set
            {
                SelectedTourLog.Rating = value;
                RaisePropertyChanged(nameof(SelectedRating));
            }
        }


        public List<Difficulty> Difficulties { get; set; }
        public List<Rating> Ratings { get; set; }

        private readonly Action<TourLog> _saveCallback;


        public EditTourLogViewModel(TourLog selectedTourLog, Action<TourLog> saveCallback)
        {
            SelectedTourLog = selectedTourLog ?? new TourLog();
            _saveCallback = saveCallback;

            // Initialize enums
            Difficulties = new List<Difficulty> { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard };
            Ratings = new List<Rating> { Rating.Bad, Rating.Okay, Rating.Good, Rating.Great, Rating.Amazing };
        }


        public ICommand ExecuteSave => new RelayCommand(_ =>
        {
            // Trigger the SaveChanges event
            _saveCallback?.Invoke(SelectedTourLog);

            // Close the window
            CloseWindow();
        });


        public ICommand ExecuteCancel => new RelayCommand(_ =>
        {
            // Close the window
            CloseWindow();
        });


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