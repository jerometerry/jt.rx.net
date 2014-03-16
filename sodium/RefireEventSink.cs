namespace Sodium
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// RefireEventSink is an EventSink that refires values that have been fired in the current
    /// Transaction when subscribed to.
    /// </summary>
    /// <typeparam name="T">The type of values fired through the Event</typeparam>
    public class RefireEventSink<T> : EventSink<T>
    {
        /// <summary>
        /// List of values that have been fired on the current Event in the current transaction.
        /// Any subscriptions that are registered in the current transaction will get fired
        /// these values on registration.
        /// </summary>
        private readonly List<T> firings = new List<T>();

        internal override bool Fire(T firing, Transaction transaction)
        {
            this.ScheduleClearFirings(transaction);
            this.AddFiring(firing);
            this.FireSubscriptionCallbacks(firing, transaction);
            return true;
        }

        /// <summary>
        /// Anything fired already in this transaction must be re-fired now so that
        /// there's no order dependency between Fire and Subscribe.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="subscription"></param>
        internal virtual bool Refire(ISubscription<T> subscription, Transaction transaction)
        {
            var toFire = this.firings;
            this.Fire(subscription, toFire, transaction);
            return true;
        }

        internal override ISubscription<T> CreateSubscription(ISodiumCallback<T> source, Rank superior, Transaction transaction)
        {
            var subscription = base.CreateSubscription(source, superior, transaction);
            this.Refire(subscription, transaction);
            return subscription;
        }

        private void ScheduleClearFirings(Transaction transaction)
        {
            var noFirings = !this.firings.Any();
            if (noFirings)
            {
                transaction.Medium(() => this.firings.Clear());
            }
        }

        private void AddFiring(T firing)
        {
            this.firings.Add(firing);
        }
    }
}