namespace Sodium
{
    /// <summary>
    /// InitialFireEventSink is an EventSink that is fired some initial values 
    /// when subscribed to.
    /// </summary>
    /// <typeparam name="T">The type of values fired through the Event</typeparam>
    /// <remarks>Used by Behavior to support firing of initial values of the Behavior</remarks>
    public abstract class InitialFireSink<T> : RefireSink<T>, IInitialFiringsEvent<T>
    {
        /// <summary>
        /// Gets the values that will be sent to newly added
        /// </summary>
        /// <returns>An Array of values that will be fired to all registered subscriptions</returns>
        /// <remarks>InitialFirings is used to support initial firings of behaviors when 
        /// the underlying event is subscribed to</remarks>
        public virtual T[] InitialFirings()
        {
            return null;
        }

        internal static TF[] GetInitialFirings<TF>(IEvent<TF> source)
        {
            var sink = source as IInitialFiringsEvent<TF>;
            return sink == null ? null : sink.InitialFirings();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="transaction"></param>
        protected override void OnSubscribe(ISubscription<T> subscription, Transaction transaction)
        {
            this.InitialFire(subscription, transaction);
            base.OnSubscribe(subscription, transaction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="transaction"></param>
        private void InitialFire(ISubscription<T> subscription, Transaction transaction)
        {
            var toFire = this.InitialFirings();
            this.Fire(subscription, toFire, transaction);
        }
    }
}