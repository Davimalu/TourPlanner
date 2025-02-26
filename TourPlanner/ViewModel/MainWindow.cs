using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TourPlanner.Logic;
using TourPlanner.Logic.MainWindow;
using TourPlanner.Model;

namespace TourPlanner.ViewModel
{
    public class MainWindow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private List<Tour> _logs;
        public List<Tour> Logs
        {
            get => _logs;
            set
            {
                _logs = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Logs)));
            }
        }

        private List<string> _tours;

        public List<string> Tours
        {
            get => _tours;
            set
            {
                _tours = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tours)));
            }
        }

        public ICommand ExecuteCommandOpenFileButtonContextMenu { get; }
        public ICommand ExecuteCommandExit { get; } = new RelayCommand(_ => Environment.Exit(0));

        public MainWindow()
        {
            ExecuteCommandOpenFileButtonContextMenu = new ExecuteCommandOpenFileButtonContextMenu(this);

            Logs = new List<Tour>
            {
                new Tour { Date = DateTime.Now, Duration = 120, Distance = 10 },
                new Tour { Date = DateTime.Now, Duration = 90, Distance = 8 },
                new Tour { Date = DateTime.Now, Duration = 60, Distance = 5 },
                new Tour { Date = DateTime.Now, Duration = 30, Distance = 2 },
                new Tour { Date = DateTime.Now, Duration = 15, Distance = 1 }
            };

            Tours = new List<string>
            {
                "Wienerwald",
                "Dopplerhütte",
                "Figlwarte",
                "Dorfrunde",
                "Vestibulum at eros"
            };
        }
    }
}
