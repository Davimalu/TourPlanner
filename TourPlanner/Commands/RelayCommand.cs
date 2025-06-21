using System.Windows.Input;

namespace TourPlanner.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute; // Reference to the method to execute when the command is invoked
        private readonly Func<object?, bool> _canExecute; // Reference to the method that determines if the command can execute

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

        
        /// <summary>
        /// event that is raised when the ability to execute the command changes
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        
        /// <summary>
        /// whether the command is currently allowed to execute
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            return _canExecute(parameter);
        }

        /// <summary>
        /// The code to execute when the command is invoked
        /// </summary>
        public void Execute(object? parameter)
        {
            // Calls the stored execute action to perform the actual command logic
            _execute(parameter);
        }
    }
}
