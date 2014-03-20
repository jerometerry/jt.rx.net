﻿namespace Sodium
{
    /// <summary>
    /// An Event is a discrete signal of values.
    /// </summary>
    /// <typeparam name="T">The type of value that will be published through the Event</typeparam>
    public class Event<T> : Observable<T>
    {
    }
}
