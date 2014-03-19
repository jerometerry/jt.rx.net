namespace Sodium
{
    using System;

    /// <summary>
    /// ISubscription is the return value of Event.Subscribe, and is used to stop 
    /// the Event from publishing the action that was passed to the Listen method.
    /// </summary>
    /// <typeparam name="T">The type of values that are published through the event being subscribed to.</typeparam>
    public interface ISubscription<T> : IDisposable
    {
        /// <summary>
        /// The notification that will be sent when the subscribed Observable publishes
        /// </summary>
        IPublisher<T> Notification { get; }

        /// <summary>
        /// Gets the IObserverable the current ISubscription is subscribed to
        /// </summary>
        IObservable<T> Source { get; }

        /// <summary>
        /// Cancels the current subscription
        /// </summary>
        void Cancel();
    }
}