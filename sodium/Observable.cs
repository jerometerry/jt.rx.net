﻿namespace Sodium
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Observable is the base class for Events and Behaviors, containing the subscription logic (i.e. the Observer Pattern).
    /// </summary>
    /// <typeparam name="T">The type of value fired through the Observable</typeparam>
    public class Observable<T> : TransactionalObject, IObservable<T>
    {
        /// <summary>
        /// The rank of the current Event. Default to rank zero
        /// </summary>
        private readonly Rank rank = new Rank();

        /// <summary>
        /// List of ISubscriptions that are currently listening for firings 
        /// from the current Event.
        /// </summary>
        private readonly List<ISubscription<T>> subscriptions = new List<ISubscription<T>>();

        /// <summary>
        /// The current Rank of the Event, used to prioritize firings on the current transaction.
        /// </summary>
        protected Rank Rank
        {
            get
            {
                return this.rank;
            }
        }

        /// <summary>
        /// List of ISubscriptions that are currently listening for firings 
        /// from the current Event.
        /// </summary>
        protected List<ISubscription<T>> Subscriptions
        {
            get
            {
                return this.subscriptions;
            }
        }

        /// <summary>
        /// Listen for firings of this event.
        /// </summary>
        /// <param name="callback">An Action to be invoked when the current Event fires.</param>
        /// <returns>An ISubscription, that should be Disposed when no longer needed. </returns>
        public ISubscription<T> Subscribe(Action<T> callback)
        {
            return this.Subscribe(new SodiumCallback<T>((a, t) => callback(a)), Rank.Highest);
        }

        /// <summary>
        /// Listen for firings of this event.
        /// </summary>
        /// <param name="callback">An Action to be invoked when the current Event fires.</param>
        /// <returns>An ISubscription, that should be Disposed when no longer needed. </returns>
        public ISubscription<T> Subscribe(ISodiumCallback<T> callback)
        {
            return this.Subscribe(callback, Rank.Highest);
        }

        /// <summary>
        /// Listen for firings on the current event
        /// </summary>
        /// <param name="callback">The action to invoke on a firing</param>
        /// <param name="subscriptionRank">A rank that will be added as a superior of the Rank of the current Event</param>
        /// <returns>An ISubscription to be used to stop listening for events</returns>
        /// <remarks>TransactionContext.Current.Run is used to invoke the overload of the 
        /// Subscribe operation that takes a thread. This ensures that any other
        /// actions triggered during Subscribe requiring a transaction all get the same instance.</remarks>
        public ISubscription<T> Subscribe(ISodiumCallback<T> callback, Rank subscriptionRank)
        {
            return this.StartTransaction(t => this.Subscribe(callback, subscriptionRank, t));
        }

        /// <summary>
        /// Listen for firings on the current event
        /// </summary>
        /// <param name="transaction">Transaction to send any firings on</param>
        /// <param name="callback">The action to invoke on a firing</param>
        /// <param name="superior">A rank that will be added as a superior of the Rank of the current Event</param>
        /// <returns>An ISubscription to be used to stop listening for events.</returns>
        /// <remarks>Any firings that have occurred on the current transaction will be re-fired immediate after subscribing.</remarks>
        public ISubscription<T> Subscribe(ISodiumCallback<T> callback, Rank superior, Transaction transaction)
        {
            return this.CreateSubscription(callback, superior, transaction);
        }

        /// <summary>
        /// Stop the given subscription from receiving updates from the current Event
        /// </summary>
        /// <param name="subscription">The subscription to remove</param>
        /// <returns>True if the subscription was removed, false otherwise</returns>
        public bool CancelSubscription(ISubscription<T> subscription)
        {
            if (subscription == null)
            {
                return false;
            }

            var l = (Subscription<T>)subscription;

            lock (Constants.SubscriptionLock)
            {
                Rank.RemoveSuperior(l.Rank);
                return this.Subscriptions.Remove(l);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="transaction"></param>
        protected virtual void OnSubscribe(ISubscription<T> subscription, Transaction transaction)
        {
        }

        /// <summary>
        /// Cleanup the current Event, disposing of any subscriptions.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            var clone = new List<ISubscription<T>>(this.Subscriptions);
            this.Subscriptions.Clear();
            foreach (var subscription in clone)
            {
                subscription.Dispose();
            }

            base.Dispose(disposing);
        }

        private ISubscription<T> CreateSubscription(ISodiumCallback<T> source, Rank superior, Transaction transaction)
        {
            Subscription<T> subscription;
            lock (Constants.SubscriptionLock)
            {
                if (this.rank.AddSuperior(superior))
                {
                    transaction.Reprioritize = true;
                }

                subscription = new Subscription<T>(this, source, superior);
                this.Subscriptions.Add(subscription);
            }

            this.OnSubscribe(subscription, transaction);
            return subscription;
        }
    }
}