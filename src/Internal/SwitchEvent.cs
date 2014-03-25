﻿namespace Potassium.Internal
{
    using Potassium.Core;

    /// <summary>
    /// SwitchEvent is an EventPublisher that publishes new values whenever the current Event in the 
    /// Behavior of Events fires.
    /// </summary>
    /// <typeparam name="T">The type of the Event</typeparam>
    /// <remarks>SwitchEvent works by subscribing to the Behaviors Event at the time of construction,
    /// then recreating the subscription when new Events are published to the Behavior (aka switching).</remarks>
    internal sealed class SwitchEvent<T> : EventPublisher<T>
    {
        private ISubscription<Event<T>> behaviorSubscription;
        private ISubscription<T> eventSubscription;
        private Observer<T> observer;

        public SwitchEvent(Behavior<Event<T>> source, Transaction transaction)
        {
            CreateBehaviorSubscription(source, transaction);
            CreateEventSubscription(source, transaction);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CancelBehaviorSubscription();
                CancelEventSubscription();
                observer = null;
            }

            base.Dispose(disposing);
        }

        private void CreateBehaviorSubscription(Behavior<Event<T>> source, Transaction transaction)
        {
            behaviorSubscription = source.Source.Subscribe(
                new Observer<Event<T>>((e, t) => 
                    t.Medium(() =>
                    {
                        CancelEventSubscription();
                        CreateNewSubscription(e, t);
                    })),
                Priority,
                transaction);
        }

        private void CreateEventSubscription(Behavior<Event<T>> source, Transaction transaction)
        {
            observer = CreateObserver();
            eventSubscription = source.Value.Subscribe(observer, Priority, transaction);
        }

        private void CreateNewSubscription(Observable<T> newEvent, Transaction transaction)
        {
            // Using a SuppressedSubscribeEvent so that no spurious Events are published.
            var suppressed = new SuppressedSubscribeEvent<T>(newEvent);

            // Subscribe to the new event, suppressing publish on subscribe
            eventSubscription = suppressed.Subscribe(observer, Priority, transaction);
         
            // Register the suppressed Event for disposable on the subscription, so that 
            // we don't need to dispose of two objects when cleaning up when a new Event 
            // is published to the Behavior.
            ((Disposable)eventSubscription).Register(suppressed);
        }

        private void CancelEventSubscription()
        {
            if (eventSubscription != null)
            {
                eventSubscription.Dispose();
                eventSubscription = null;
            }
        }

        private void CancelBehaviorSubscription()
        {
            if (behaviorSubscription != null)
            {
                behaviorSubscription.Dispose();
                behaviorSubscription = null;
            }
        }
    }
}
