﻿namespace Sodium
{
    /// <summary>
    /// A Behavior who's value never changes, even if published new values.
    /// </summary>
    /// <typeparam name="T">The type of values published through the Behavior</typeparam>
    public class ConstantBehavior<T> : Behavior<T>
    {
        /// <summary>
        /// Constructs a new ConstantBehavior
        /// </summary>
        /// <param name="initValue">The constant initial value of the Behavior</param>
        public ConstantBehavior(T initValue)
            : base(new Event<T>(), initValue)
        {
            this.Register(this.Source);
        }
    }
}
