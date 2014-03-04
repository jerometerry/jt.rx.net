namespace Sodium
{
    using System;

    /// <summary>
    /// IEventListener is the return value of Event.Listen, and is used to stop 
    /// the Event from firing the action that was passed to the Listen method.
    /// </summary>
    /// <typeparam name="T">The type of values that are fired through the event being listened to.</typeparam>
    /// <remarks>To stop the IEventListener from receiving updates from the Event,
    /// call the Dispose method. The event will be disposed when the number of listeners reaches zero.
    /// </remarks>
    public interface IEventListener<T> : IDisposable
    {
        /// <summary>
        /// Gets the Event the current IEventListener is listening to
        /// </summary>
        Event<T> Source { get; }
    }
}