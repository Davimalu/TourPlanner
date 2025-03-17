using System.Windows.Input;

namespace TourPlanner.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool> _canExecute;

        // Constructor 
        public RelayCommand(Action<object?> execute, Func<object?, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Constructor - if no canExecute is provided, set it to true
        public RelayCommand(Action<object?> execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = _ => true;
        }

        // The ICommand interface requires this event:
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

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
