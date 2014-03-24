﻿namespace Potassium.Internal
{
    using Potassium.Core;

    internal sealed class SwitchEvent<T> : EventPublisher<T>
    {
        private ISubscription<Event<T>> behaviorSubscription;
        private SubscriptionPublisher<T> wrappedEventSubscriptionCallback;
        private ISubscription<T> wrappedSubscription;
        private Behavior<Event<T>> source;

        public SwitchEvent(Behavior<Event<T>> source, Transaction transaction)
        {
            this.source = source;
            this.Initialize(transaction);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.behaviorSubscription != null)
            {
                this.behaviorSubscription.Dispose();
                this.behaviorSubscription = null;
            }

            if (this.wrappedSubscription != null)
            {
                this.wrappedSubscription.Dispose();
                this.wrappedSubscription = null;
            }

            this.wrappedEventSubscriptionCallback = null;
            this.source = null;

            base.Dispose(disposing);
        }

        private void Initialize(Transaction transaction)
        {
            this.wrappedEventSubscriptionCallback = this.CreateSubscriptionPublisher();
            this.wrappedSubscription = source.Value.CreateSubscription(this.wrappedEventSubscriptionCallback, this.Priority, transaction);

            var behaviorEventChanged = new SubscriptionPublisher<Event<T>>(this.UpdateWrappedEventSubscription);
            this.behaviorSubscription = source.Source.CreateSubscription(behaviorEventChanged, this.Priority, transaction);

            return;
        }

        private void UpdateWrappedEventSubscription(Event<T> newEvent, Transaction transaction)
        {
            transaction.Medium(() =>
            {
                if (this.wrappedSubscription != null)
                {
                    this.wrappedSubscription.Dispose();
                    this.wrappedSubscription = null;
                }

                var suppressed = new SuppressedSubscribeEvent<T>(newEvent);
                this.wrappedSubscription = suppressed.CreateSubscription(this.wrappedEventSubscriptionCallback, this.Priority, transaction);
                ((Disposable)this.wrappedSubscription).Register(suppressed);
            });
        }
    }
}
