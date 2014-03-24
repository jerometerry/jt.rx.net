﻿namespace Potassium.Core
{
    using System;
    using Potassium.Internal;
    using Potassium.Providers;

    /// <summary>
    /// An Event is a discrete signal of values.
    /// </summary>
    /// <typeparam name="T">The type of value that will be published through the Event</typeparam>
    public class Event<T> : Observable<T>
    {
        /// <summary>
        /// If there's more than one publishing in a single transaction, combine them into
        /// one using the specified combining function.
        /// </summary>
        /// <param name="coalesce">A function that takes two publishings of the same type, and returns
        /// produces a new publishing of the same type.</param>
        /// <returns>A new Event that publishes the coalesced values</returns>
        /// <remarks>
        /// If the event publishings are ordered, then the first will appear at the left
        /// input of the combining function. In most common cases it's best not to
        /// make any assumptions about the ordering, and the combining function would
        /// ideally be commutative.
        /// </remarks>
        public Event<T> Coalesce(Func<T, T, T> coalesce)
        {
            return Transaction.Start(t => new CoalesceEvent<T>(this, coalesce, t));
        }

        /// <summary>
        /// Only keep event occurrences for which the predicate returns true.
        /// </summary>
        /// <param name="predicate">A predicate used to include publishings</param>
        /// <returns>A new Event that is published when the current Event publishes and
        /// the predicate evaluates to true.</returns>
        public Event<T> Filter(Func<T, bool> predicate)
        {
            return new FilterEvent<T>(this, predicate);
        }

        /// <summary>
        /// Filter out any event occurrences whose value is null.
        /// </summary>
        /// <returns>A new Event that publishes whenever the current Event publishes with a non-null value</returns>
        /// <remarks>For value types, comparison against null will always be false. 
        /// FilterNotNull will not filter out any values for value types.</remarks>
        public Event<T> FilterNotNull()
        {
            return new FilterEvent<T>(this, a => a != null);
        }

        /// <summary>
        /// Create a behavior with the specified initial value, that gets updated
        /// by the values coming through the event. The 'current value' of the behavior
        /// is notionally the value as it was 'at the start of the transaction'.
        /// That is, state updates caused by event publishings get processed at the end of
        /// the transaction.
        /// </summary>
        /// <param name="value">The initial value for the Behavior</param>
        /// <returns>A Behavior that updates when the current event is published,
        /// having the specified initial value.</returns>
        public Behavior<T> Hold(T value)
        {
            return Transaction.Start(t => new HoldBehavior<T>(this, value, t));
        }

        /// <summary>
        /// Gets an Event that only fires once if the current Event fires multiple times in the same transaction
        /// </summary>
        /// <returns>The Event that fires only the last event in a Transaction</returns>
        public Event<T> LastFiring()
        {
            return Transaction.Start(t => new LastFiringEvent<T>(this, t));
        }

        /// <summary>
        /// Map publishings of the current event using the supplied mapping function.
        /// </summary>
        /// <typeparam name="TB">The return type of the map</typeparam>
        /// <param name="map">A map from T -> TB</param>
        /// <returns>A new Event that publishes whenever the current Event publishes, the
        /// the mapped value is computed using the supplied mapping.</returns>
        public Event<TB> Map<TB>(Func<T, TB> map)
        {
            return new MapEvent<T, TB>(this, map);
        }

        /// <summary>
        /// Merge two streams of events of the same type.
        /// </summary>
        /// <param name="observable">The Event to merge with the current Event</param>
        /// <returns>A new Event that publishes whenever either the current or source Events publish.</returns>
        /// <remarks>
        /// In the case where two event occurrences are simultaneous (i.e. both
        /// within the same transaction), both will be delivered in the same
        /// transaction. If the event publishings are ordered for some reason, then
        /// their ordering is retained. In many common cases the ordering will
        /// be undefined.
        /// </remarks>
        public Event<T> Merge(Observable<T> observable)
        {
            return new MergeEvent<T>(this, observable);
        }

        /// <summary>
        /// Throw away all event occurrences except for the first one.
        /// </summary>
        /// <returns>An Event that only publishes one time, the first time the current event publishes.</returns>
        public Event<T> Once()
        {
            return new OnceEvent<T>(this);
        }

        /// <summary>
        /// Sample the behavior at the time of the event publishing. 
        /// </summary>
        /// <typeparam name="TB">The type of the Behavior</typeparam>
        /// <typeparam name="TC">The return type of the snapshot function</typeparam>
        /// <param name="snapshot">The snapshot generation function.</param>
        /// <param name="provider">The Behavior to sample when calculating the snapshot</param>
        /// <returns>A new Event that will produce the snapshot when the current event publishes</returns>
        /// <remarks>Note that the 'current value' of the behavior that's sampled is the value 
        /// as at the start of the transaction before any state changes of the current transaction 
        /// are applied through 'hold's.</remarks>
        public Event<TC> Snapshot<TB, TC>(Func<T, TB, TC> snapshot, IProvider<TB> provider)
        {
            return new SnapshotEvent<T, TB, TC>(this, snapshot, provider);
        }

        /// <summary>
        /// Sample the providers value at the time of the event publishing
        /// </summary>
        /// <typeparam name="TB">The type of the Provider</typeparam>
        /// <param name="provider">The IProvider to sample when taking the snapshot</param>
        /// <returns>An event that captures the IProviders value when the current event publishes</returns>
        public Event<TB> Snapshot<TB>(IProvider<TB> provider)
        {
            return this.Snapshot((a, b) => b, provider);
        }
    }
}
