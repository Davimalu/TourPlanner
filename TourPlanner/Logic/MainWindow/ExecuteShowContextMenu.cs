using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Reflection;

namespace TourPlanner.Logic.MainWindow
{
    class ExecuteShowContextMenu : ICommand
    {
        private ViewModel.MainWindow _mainWindow;
        public ExecuteShowContextMenu(ViewModel.MainWindow mainWindow)
        {
            this._mainWindow = mainWindow;
        }


        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (parameter is Button btn && btn.ContextMenu != null)
            {
                // Set DataContext to the ViewModel (otherwise the binding won't work)
                btn.ContextMenu.DataContext = btn.DataContext;

                // Open the context menu
                btn.ContextMenu.IsOpen = true;
            }
        }
    }
}
