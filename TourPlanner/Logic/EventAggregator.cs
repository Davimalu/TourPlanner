using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;

namespace TourPlanner.Logic;

/// <summary>
/// <para>Event aggregator for managing event subscriptions and publications</para>
/// This implementation is NOT thread-safe - since our application is single-threaded, we omitted this to reduce complexity
/// </summary>
public class EventAggregator : IEventAggregator
{
    private readonly ILogger<EventAggregator> _logger;
    
    public EventAggregator(ILogger<EventAggregator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    // Maps event types to their subscribers
    // Key: The Type of event (e.g., ToursChangedEvent)
    // Value: List of Delegate handlers that want to receive that event type
    private readonly Dictionary<Type, List<Delegate>> _eventSubscribers = new Dictionary<Type, List<Delegate>>();
    
    
    public void Publish<T>(T eventData) where T : class
    {
        // Get all subscribers (handlers) for this type of event
        List<Delegate>? eventHandlers = null;
        
        if (_eventSubscribers.TryGetValue(typeof(T), out var handlers))
        {
            // Create a copy of the handlers list to avoid issues if someone subscribes/unsubscribes while we're iterating through the list
            eventHandlers = handlers.ToList();
        }
        
        // If we found any handlers for this type of event, call them all
        if (eventHandlers != null && eventHandlers.Count > 0)
        {
            foreach (var currentHandler  in eventHandlers)
            {
                try
                {
                    // Cast the generic Delegate back to the specific Action<T> type and invoke it with the event data
                    var typedHandler = (Action<T>)currentHandler;
                    typedHandler(eventData);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error handling event {typeof(T).Name}: {ex.Message}", ex);
                }
            }
        }
    }


    public void Subscribe<T>(Action<T> handler) where T : class
    {
        // Don't allow null handlers
        if (handler == null)
        {
            return;
        }
        
        // Try to get the existing list of handlers for this event type
        if (!_eventSubscribers.TryGetValue(typeof(T), out var existingHandlers))
        {
            // If no handlers exist for this event type yet, create a new list
            existingHandlers = new List<Delegate>();
            _eventSubscribers[typeof(T)] = existingHandlers;
        }
        
        // Add this handler to the list
        existingHandlers.Add(handler);
    }

    
    public void Unsubscribe<T>(Action<T> handler) where T : class
    {
        // Don't allow null handlers
        if (handler == null)
        {
            return;
        }
        
        // Try to find the list of handlers for this event type
        if (_eventSubscribers.TryGetValue(typeof(T), out var existingHandlers))
        {
            // Remove the specific handler from the list
            existingHandlers.Remove(handler);
            
            // If the list is now empty, remove the entire entry to keep the dictionary clean
            if (existingHandlers.Count == 0)
            {
                _eventSubscribers.Remove(typeof(T));
            }
        }
    }
}