﻿namespace Sodium
{
    internal sealed class SwitchEvent<T> : EventPublisher<T>
    {
        private ISubscription<Event<T>> behaviorSubscription;
        private IPublisher<T> wrappedEventSubscriptionCallback;
        private ISubscription<T> wrappedSubscription;
        private Behavior<Event<T>> source;

        public SwitchEvent(Behavior<Event<T>> source)
        {
            this.source = source;
            this.StartTransaction(this.Initialize);
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

        private Unit Initialize(Transaction transaction)
        {
            this.wrappedEventSubscriptionCallback = this.CreatePublisher();
            this.wrappedSubscription = source.Value.Subscribe(this.wrappedEventSubscriptionCallback, this.Rank, transaction);

            var behaviorEventChanged = new Publisher<Event<T>>(this.UpdateWrappedEventSubscription);
            this.behaviorSubscription = source.Subscribe(behaviorEventChanged, this.Rank, transaction);

            return Unit.Nothing;
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
                this.wrappedSubscription = suppressed.Subscribe(this.wrappedEventSubscriptionCallback, this.Rank, transaction);
                ((DisposableObject)this.wrappedSubscription).Register(suppressed);
            });
        }
    }
}
