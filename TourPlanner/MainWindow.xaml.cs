using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TourPlanner;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private List<TourLog> logs;
    public List<TourLog> Logs
    {
        get => logs;
        set
        {
            logs = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Logs)));
        }
    }

    private List<string> tours;

    public List<string> Tours
    {
        get => tours;
        set
        {
            tours = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tours)));
        }
    }

    public MainWindow()
    {
        InitializeComponent();

        Logs = new List<TourLog>
        {
            new TourLog { Date = DateTime.Now, Duration = 120, Distance = 10 },
            new TourLog { Date = DateTime.Now, Duration = 90, Distance = 8 },
            new TourLog { Date = DateTime.Now, Duration = 60, Distance = 5 },
            new TourLog { Date = DateTime.Now, Duration = 30, Distance = 2 },
            new TourLog { Date = DateTime.Now, Duration = 15, Distance = 1 }
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


    private void FileButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.ContextMenu != null)
        {
            btn.ContextMenu.IsOpen = true;
        }
    }


    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}

public class TourLog
{
    public DateTime Date { get; set; }
    public int Duration { get; set; }
    public int Distance { get; set; }
}