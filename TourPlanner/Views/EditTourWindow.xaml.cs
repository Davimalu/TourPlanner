using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace TourPlanner.Views
{
    /// <summary>
    /// Interaction logic for EditTourWindow.xaml
    /// </summary>
    public partial class EditTourWindow : Window
    {
        public EditTourWindow()
        {
            InitializeComponent();
            
            // Set the DataContext for the Map
            Map.DataContext = App.ServiceProvider.GetRequiredService<ViewModels.MapViewModel>();
        }
    }
}
