﻿namespace Potassium.Extensions
{
    using System;
    using Potassium.Core;

    /// <summary>
    /// Event extension methods
    /// </summary>
    public static class EventExtensions
    {
        /// <summary>
        /// Accumulate on input event, outputting the new state each time.
        /// </summary>
        /// <typeparam name="T">The type of the source Event</typeparam>
        /// <typeparam name="TS">The return type of the snapshot function</typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="value">The initial state of the behavior</param>
        /// <param name="accumulator">The snapshot generation function</param>
        /// <returns>A new Behavior starting with the given value, that updates 
        /// whenever the current event publishes, getting a value computed by the snapshot function.</returns>
        public static Behavior<TS> Accum<T, TS>(this Event<T> source, Func<T, TS, TS> accumulator, TS value)
        {
            EventFeed<TS> eventFeed = new EventFeed<TS>();

            // Behavior holds the running snapshot value
            Behavior<TS> previousShapshotBehavior = eventFeed.Hold(value);

            // Event that fires the new accumulated values, using the accumulator and the previous values
            Event<TS> accumulationEvent = source.Snapshot(accumulator, previousShapshotBehavior);

            // Feed the new accumulated values into the Behavior, to store the new snapshot as the previous snapshot
            eventFeed.Feed(accumulationEvent);

            // Behavior that holds the value of the new accumulated values
            Behavior<TS> result = accumulationEvent.Hold(value);

            result.Register(eventFeed);
            result.Register(previousShapshotBehavior);
            result.Register(accumulationEvent);

            return result;
        }

        /// <summary>
        /// Transform an event with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        /// <typeparam name="T">The type of the source Event</typeparam>
        /// <typeparam name="TB">The return type of the new Event</typeparam>
        /// <typeparam name="TS">The snapshot type</typeparam>
        /// <param name="source">The source Event</param>
        /// <param name="initState">The initial state for the internal Behavior</param>
        /// <param name="collector">The mealy machine</param>
        /// <returns>An Event that collects new values</returns>
        /// <remarks>Collect is similar to Accum, execpt the return value of the supplied function
        /// is a Tuple containing an intermediate value along with the snapshot value.</remarks>
        public static Event<TB> Collect<T, TB, TS>(this Event<T> source, Func<T, TS, Tuple<TB, TS>> collector, TS initState)
        {
            // snapshotFeed is used to create the Behavior that holds the snapshot values
            EventFeed<TS> snapshotFeed = new EventFeed<TS>();

            // Behavior that holds the previous collected value
            Behavior<TS> snapshotBehavior = snapshotFeed.Hold(initState);

            // Event that emits a Tuple<TB,TS> containing the mapped value and the snapshot
            Event<Tuple<TB, TS>> mappedEventSnapshot = source.Snapshot(collector, snapshotBehavior);

            // Event that emits the snapshot values from the mappedEventSnapshot above
            Event<TS> snapshotEvent = mappedEventSnapshot.Map(bs => bs.Item2);

            // Feed the snapshots into the Behavior holding the snapshot values
            snapshotFeed.Feed(snapshotEvent);

            // Event that extracts the mapped value from the mappedEventSnapshot above
            Event<TB> mappedEvent = mappedEventSnapshot.Map(bs => bs.Item1);

            mappedEvent.Register(snapshotFeed);
            mappedEvent.Register(snapshotBehavior);
            mappedEvent.Register(mappedEventSnapshot);
            mappedEvent.Register(snapshotEvent);

            return mappedEvent;
        }
    }
}
