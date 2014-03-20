namespace Sodium
{
    using System;

    /// <summary>
    /// A Behavior is a continuous, time varying value. It starts with an initial value which 
    /// gets updated as the underlying Event is published.
    /// </summary>
    /// <typeparam name="T">The type of values that will be published through the Behavior.</typeparam>
    /// <remarks> In theory, a Behavior is a continuous value, whereas an Event is a discrete sequence of values.
    /// In Sodium.net, a Behavior is implemented by observing the discrete values of an Event, so a Behavior
    /// technically isn't continuous.
    /// </remarks>
    public class EventBasedBehavior<T> : Behavior<T>
    {
        /// <summary>
        /// Create a behavior with a time varying value from an initial value
        /// </summary>
        /// <param name="value">The initial value of the Behavior</param>
        public EventBasedBehavior(T value)
            : this(new Event<T>(), value)
        {
            this.Register(this.Source);
        }

        /// <summary>
        /// Create a behavior with a time varying value from an Event and an initial value
        /// </summary>
        /// <param name="source">The Observable to listen for updates from</param>
        /// <param name="value">The initial value of the Behavior</param>
        public EventBasedBehavior(Event<T> source, T value)
        {
            this.Source = source;
            this.ObservedValue = new ObservedValue<T>(source, value);
        }

        /// <summary>
        /// Sample the behavior's current value
        /// </summary>
        /// <remarks>
        /// This should generally be avoided in favor of SubscribeValues(..) so you don't
        /// miss any updates, but in many circumstances it makes sense.
        /// 
        /// Value is the value of the Behavior at the start of a Transaction
        ///
        /// It can be best to use it inside an explicit transaction (using TransactionContext.Current.Run()).
        /// For example, a b.Value inside an explicit transaction along with a
        /// b.Source.Subscribe(..) will capture the current value and any updates without risk
        /// of missing any in between.
        /// </remarks>
        public override T Value
        {
            get
            {
                return this.ObservedValue.Value;
            }
        }

        /// <summary>
        /// The underlying Event of the current Behavior
        /// </summary>
        public Event<T> Source { get; private set; }

        /// <summary>
        /// New value of the Behavior that will be posted to Value when the Transaction completes
        /// </summary>
        internal T NewValue
        {
            get
            {
                return this.ObservedValue.NewValue;
            }
        }

        private ObservedValue<T> ObservedValue { get; set; }

        /// <summary>
        /// Dispose of the current Behavior
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (this.ObservedValue != null)
            {
                this.ObservedValue.Dispose();
                this.ObservedValue = null;
            }

            base.Dispose(disposing);
        }
    }
}