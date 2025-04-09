using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TourPlanner.DAL.ServiceAgents;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;

namespace TourPlanner.Commands
{
    public class RelayCommandAsync : ICommand
    {
        private readonly Func<object?, Task> _executeAsync;
        private readonly Func<object?, bool> _canExecute;
        private bool _isExecuting;

        private readonly ILoggerWrapper _logger;

        /// <summary>
        /// Creates a new AsyncRelayCommand.
        /// </summary>
        /// <param name="executeAsync">The asynchronous operation to run when the command is executed</param>
        /// <param name="canExecute">whether the command can be executed or not</param>
        public RelayCommandAsync(Func<object?, Task> executeAsync, Func<object?, bool>? canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute ?? (_ => true);
            _logger = LoggerFactory.GetLogger<RelayCommandAsync>();
        }

        // The ICommand interface requires this event:
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
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
                return;

            _isExecuting = true;
            try
            {
                // Calls the stored execute action to perform the actual command logic
                await _executeAsync(parameter);
            }
            catch (Exception ex)
            {
                _logger.Error($"Async command execution failed", ex);
            }
            finally
            {
                _isExecuting = false;
            }
        }
    }
}
