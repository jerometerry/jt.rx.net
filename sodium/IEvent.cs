﻿namespace Sodium
{
    using System;

    public interface IEvent<T> : IDisposableObject
    {
        /// <summary>
        /// Accumulate on input event, outputting the new state each time.
        /// </summary>
        /// <typeparam name="TS">The return type of the snapshot function</typeparam>
        /// <param name="initState">The initial state of the behavior</param>
        /// <param name="snapshot">The snapshot generation function</param>
        /// <returns>A new Behavior starting with the given value, that updates 
        /// whenever the current event fires, getting a value computed by the snapshot function.</returns>
        IBehavior<TS> Accum<TS>(TS initState, Func<T, TS, TS> snapshot);

        /// <summary>
        /// Stop the given subscription from receiving updates from the current Event
        /// </summary>
        /// <param name="subscription">The subscription to remove</param>
        /// <returns>True if the subscription was removed, false otherwise</returns>
        IEvent<T> Coalesce(Func<T, T, T> coalesce);

        /// <summary>
        /// Transform an event with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        /// <typeparam name="TB">The return type of the new Event</typeparam>
        /// <typeparam name="TS">The snapshot type</typeparam>
        /// <param name="initState">The initial state for the internal Behavior</param>
        /// <param name="snapshot">The mealy machine</param>
        /// <returns>An Event that collects new values</returns>
        IEvent<TB> Collect<TB, TS>(TS initState, Func<T, TS, Tuple<TB, TS>> snapshot);

        /// <summary>
        /// Push each event occurrence onto a new transaction.
        /// </summary>
        /// <returns>An event that is fired with the lowest priority in the current Transaction the current Event is fired in.</returns>
        IEvent<T> Delay();

        /// <summary>
        /// Only keep event occurrences for which the predicate returns true.
        /// </summary>
        /// <param name="predicate">A predicate used to include firings</param>
        /// <returns>A new Event that is fired when the current Event fires and
        /// the predicate evaluates to true.</returns>
        IEvent<T> Filter(Func<T, bool> predicate);

        /// <summary>
        /// Filter out any event occurrences whose value is null.
        /// </summary>
        /// <returns>A new Event that fires whenever the current Event fires with a non-null value</returns>
        /// <remarks>For value types, comparison against null will always be false. 
        /// FilterNotNull will not filter out any values for value types.</remarks>
        IEvent<T> FilterNotNull();

        /// <summary>
        /// Let event occurrences through only when the behavior's value is True.
        /// Note that the behavior's value is as it was at the start of the transaction,
        /// that is, no state changes from the current transaction are taken into account.
        /// </summary>
        /// <param name="predicate">A behavior who's current value acts as a predicate</param>
        /// <returns>A new Event that fires whenever the current Event fires and the Behaviors value
        /// is true.</returns>
        IEvent<T> Gate(IValue<bool> predicate);

        /// <summary>
        /// Create a behavior with the specified initial value, that gets updated
        /// by the values coming through the event. The 'current value' of the behavior
        /// is notionally the value as it was 'at the start of the transaction'.
        /// That is, state updates caused by event firings get processed at the end of
        /// the transaction.
        /// </summary>
        /// <param name="initValue">The initial value for the Behavior</param>
        /// <returns>A Behavior that updates when the current event is fired,
        /// having the specified initial value.</returns>
        IBehavior<T> Hold(T initValue);

        /// <summary>
        /// Map firings of the current event using the supplied mapping function.
        /// </summary>
        /// <typeparam name="TB">The return type of the map</typeparam>
        /// <param name="map">A map from T -> TB</param>
        /// <returns>A new Event that fires whenever the current Event fires, the
        /// the mapped value is computed using the supplied mapping.</returns>
        IEvent<TB> Map<TB>(Func<T, TB> map);

        /// <summary>
        /// Merge two streams of events of the same type.
        /// </summary>
        /// <param name="source">The Event to merge with the current Event</param>
        /// <returns>A new Event that fires whenever either the current or source Events fire.</returns>
        /// <remarks>
        /// In the case where two event occurrences are simultaneous (i.e. both
        /// within the same transaction), both will be delivered in the same
        /// transaction. If the event firings are ordered for some reason, then
        /// their ordering is retained. In many common cases the ordering will
        /// be undefined.
        /// </remarks>
        IEvent<T> Merge(IEvent<T> source);

        /// <summary>
        /// Merge two streams of events of the same type, combining simultaneous
        /// event occurrences.
        /// </summary>
        /// <param name="source">The Event to merge with the current Event</param>
        /// <param name="coalesce">The coalesce function that combines simultaneous firings.</param>
        /// <returns>An Event that is fired whenever the current or source Events fire, where
        /// simultaneous firings are handled by the coalesce function.</returns>
        /// <remarks>
        /// In the case where multiple event occurrences are simultaneous (i.e. all
        /// within the same transaction), they are combined using the same logic as
        /// 'coalesce'.
        /// </remarks>
        IEvent<T> Merge(IEvent<T> source, Func<T, T, T> coalesce);

        /// <summary>
        /// Throw away all event occurrences except for the first one.
        /// </summary>
        /// <returns>An Event that only fires one time, the first time the current event fires.</returns>
        IEvent<T> Once();

        /// <summary>
        /// Sample the behavior at the time of the event firing. Note that the 'current value'
        /// of the behavior that's sampled is the value as at the start of the transaction
        /// before any state changes of the current transaction are applied through 'hold's.
        /// </summary>
        /// <typeparam name="TB">The type of the Behavior</typeparam>
        /// <param name="valueStream">The Behavior to sample when calculating the snapshot</param>
        /// <returns>A new Event that will produce the snapshot when the current event fires</returns>
        IEvent<TB> Snapshot<TB>(IValue<TB> valueStream);

        /// <summary>
        /// Sample the behavior at the time of the event firing. Note that the 'current value'
        /// of the behavior that's sampled is the value as at the start of the transaction
        /// before any state changes of the current transaction are applied through 'hold's.
        /// </summary>
        /// <typeparam name="TB">The type of the Behavior</typeparam>
        /// <typeparam name="TC">The return type of the snapshot function</typeparam>
        /// <param name="valueStream">The Behavior to sample when calculating the snapshot</param>
        /// <param name="snapshot">The snapshot generation function.</param>
        /// <returns>A new Event that will produce the snapshot when the current event fires</returns>
        IEvent<TC> Snapshot<TB, TC>(IValue<TB> valueStream, Func<T, TB, TC> snapshot);

        /// <summary>
        /// Listen for firings of this event.
        /// </summary>
        /// <param name="callback">An Action to be invoked when the current Event fires.</param>
        /// <returns>An ISubscription, that should be Disposed when no longer needed. </returns>
        ISubscription<T> Subscribe(Action<T> callback);

        /// <summary>
        /// Listen for firings of this event.
        /// </summary>
        /// <param name="callback">An Action to be invoked when the current Event fires.</param>
        /// <returns>An ISubscription, that should be Disposed when no longer needed. </returns>
        ISubscription<T> Subscribe(ISodiumCallback<T> callback);

        /// <summary>
        /// Listen for firings on the current event
        /// </summary>
        /// <param name="callback">The action to invoke on a firing</param>
        /// <param name="subscriptionRank">A rank that will be added as a superior of the Rank of the current Event</param>
        /// <returns>An ISubscription to be used to stop listening for events</returns>
        /// <remarks>TransactionContext.Current.Run is used to invoke the overload of the 
        /// Subscribe operation that takes a thread. This ensures that any other
        /// actions triggered during Subscribe requiring a transaction all get the same instance.</remarks>
        ISubscription<T> Subscribe(ISodiumCallback<T> callback, Rank subscriptionRank);

        /// <summary>
        /// Listen for firings on the current event
        /// </summary>
        /// <param name="transaction">Transaction to send any firings on</param>
        /// <param name="callback">The action to invoke on a firing</param>
        /// <param name="superior">A rank that will be added as a superior of the Rank of the current Event</param>
        /// <returns>An ISubscription to be used to stop listening for events.</returns>
        /// <remarks>Any firings that have occurred on the current transaction will be re-fired immediate after subscribing.</remarks>
        ISubscription<T> Subscribe(ISodiumCallback<T> callback, Rank superior, Transaction transaction);

        /// <summary>
        /// Stop the given subscription from receiving updates from the current Event
        /// </summary>
        /// <param name="subscription">The subscription to remove</param>
        /// <returns>True if the subscription was removed, false otherwise</returns>
        bool CancelSubscription(ISubscription<T> subscription);
    }
}
