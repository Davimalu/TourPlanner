using System.Windows.Controls;
using System.Windows.Input;
using TourPlanner.Commands;

namespace TourPlanner.ViewModels
{
    class MenuBarViewModel : BaseViewModel
    {
        // TODO: Check if this is needed
        public ICommand ExecuteShowContextMenu { get; } = new RelayCommand(param =>
        {
            if (param is Button btn && btn.ContextMenu != null)
            {
                // Set DataContext to the ViewModel (otherwise the binding won't work)
                btn.ContextMenu.DataContext = btn.DataContext;

                // Open the context menu
                btn.ContextMenu.IsOpen = true;
            }
        });

        public ICommand ExecuteExitApplication { get; } = new RelayCommand(_ =>
        {
            Environment.Exit(0);
        });
    }
}
