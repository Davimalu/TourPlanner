using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TourPlanner.Logic
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        // If no canExecute is provided, always return true
        public RelayCommand(Action<object?> execute)
            : this(execute, _ => true)
        {
        }

        // If canExecute is provided, use it
        public RelayCommand(Action<object?> execute, Func<object?, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        // The ICommand interface requires this event | 
        public event EventHandler? CanExecuteChanged;

        // The ICommand interface requires these two methods:

        public bool CanExecute(object? parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            // Calls the stored execute action to perform the actual command logic
            _execute(parameter);
        }
    }
}
