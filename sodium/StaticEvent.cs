﻿namespace Sodium
{
    /// <summary>
    /// StaticEvent is an event that swallows all firings, preventing them from
    /// triggering callbacks on any listeners.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>The primary use case for StaticEvent is to enable a constant 
    /// valued Behavior.</remarks>
    internal sealed class StaticEvent<T> : Event<T>
    {
        /// <summary>
        /// Swallow the Firing of events
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="firing"></param>
        internal override void Fire(Transaction transaction, T firing)
        {
        }
    }
}