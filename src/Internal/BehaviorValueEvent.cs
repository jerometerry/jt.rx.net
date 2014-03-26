namespace Potassium.Internal
{
    using Potassium.Core;

    /// <summary>
    /// BehaviorValueEvent is an Event that fires the current value when subscribed to,
    /// and fires all updates thereafter.
    /// </summary>
    /// <typeparam name="T">The type of values fired through the Behavior</typeparam>
    internal sealed class BehaviorValueEvent<T> : FireEvent<T>
    {
        private Behavior<T> behavior;
        private ISubscription<T> subscription;

        /// <summary>
        /// Constructs a new BehaviorEventSink
        /// </summary>
        /// <param name="behavior">The Behavior to monitor</param>
        /// <param name="transaction">The Transaction to use to create a subscription on the given Behavior</param>
        public BehaviorValueEvent(Behavior<T> behavior, Transaction transaction)
        {
            this.behavior = behavior;
            this.CreateLoop(transaction);
        }

        public override T[] SubscriptionFirings()
        {
            // When the ValueEventSink is subscribed to, fire off the current value of the Behavior
            return new[] { this.behavior.Value };
        }

        protected override void Dispose(bool disposing)
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }

            this.behavior = null;

            base.Dispose(disposing);
        }

        /// <summary>
        /// Forward firings from the behavior to the current BehaviorEventSink
        /// </summary>
        /// <param name="transaction"></param>
        private void CreateLoop(Transaction transaction)
        {
            var source = this.behavior.Source;
            var forward = this.Forward();
            this.subscription = source.Subscribe(forward, this.Priority, transaction);
        }
    }
}