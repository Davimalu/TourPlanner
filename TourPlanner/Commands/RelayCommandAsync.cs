using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using TourPlanner.Infrastructure.Interfaces;

namespace TourPlanner.Commands
{
    public class RelayCommandAsync : ICommand
    {
        private readonly Func<object?, Task> _executeAsync;
        private readonly Func<object?, bool> _canExecute;
        private bool _isExecuting;

        private readonly ILogger<RelayCommandAsync>? _logger;

        /// <summary>
        /// Creates a new AsyncRelayCommand.
        /// </summary>
        /// <param name="executeAsync">The asynchronous operation to run when the command is executed</param>
        /// <param name="canExecute">whether the command can be executed or not</param>
        public RelayCommandAsync(Func<object?, Task> executeAsync, Func<object?, bool>? canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute ?? (_ => true);

            // In Unit Tests, the Service Provider might not be available, so we check for null
            if (App.ServiceProvider != null)
            {
                _logger = App.ServiceProvider.GetRequiredService<ILogger<RelayCommandAsync>>();
            }
        }

        /// <summary>
        /// event that is raised when the ability to execute the command changes
        /// </summary>
        public event EventHandler? CanExecuteChanged;
        
        /// <summary>
        /// Raises the CanExecuteChanged event to notify that the command's ability to execute has changed.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {        
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Determines whether the command can execute in its current state
        /// Prevents re-entry if an async operation is still running
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && _canExecute(parameter);
        }

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }
            _isExecuting = true;
            RaiseCanExecuteChanged(); // Notify that the command can no longer execute
            
            try
            {
                // Calls the stored execute action to perform the actual command logic
                await _executeAsync(parameter);
            }
            catch (Exception ex)
            {
                _logger?.Error($"Async command execution failed", ex);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged(); // Notify that the command can now execute again
            }
        }
        
        
        /// <summary>
        /// Executes the command asynchronously and returns a Task (used for UnitTesting - not compatible with the void return type of ICommand.Execute).
        /// </summary>
        public async Task ExecuteAsync(object? parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }
            _isExecuting = true;
            RaiseCanExecuteChanged();
        
            try
            {
                await _executeAsync(parameter);
            }
            catch (Exception ex)
            {
                _logger?.Error($"Async command execution failed", ex);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }
    }
}
