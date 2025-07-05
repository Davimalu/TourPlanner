namespace TourPlanner.Logic.Interfaces;

public interface IEventAggregator
{
    /// <summary>
    /// Publishes an event to all subscribers of that event type
    /// </summary>
    /// <typeparam name="T">The type of event being published</typeparam>
    /// <param name="eventData">The actual event data/message to send</param>
    void Publish<T>(T eventData) where T : class;
    
    /// <summary>
    /// <para>Subscribes a handler method to receive events of a specific type.</para>
    /// When an event of type T is published, the provided handler will be called.
    /// </summary>
    /// <typeparam name="T">The type of event to subscribe to</typeparam>
    /// <param name="handler">The method that will handle the event when it's published</param>
    void Subscribe<T>(Action<T> handler) where T : class;
    
    /// <summary>
    /// Unsubscribes a handler from receiving events of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of event to unsubscribe from</typeparam>
    /// <param name="handler">The specific handler method to remove</param>
    void Unsubscribe<T>(Action<T> handler) where T : class;
}