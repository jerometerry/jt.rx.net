﻿namespace Sodium
{
    /// <summary>
    /// A Behavior who's value never changes, even if published new values.
    /// </summary>
    /// <typeparam name="T">The type of values published through the Behavior</typeparam>
    public class ConstantBehavior<T> : Behavior<T>
    {
        private T value;

        /// <summary>
        /// Constructs a new ConstantBehavior
        /// </summary>
        /// <param name="value">The constant initial value of the Behavior</param>
        public ConstantBehavior(T value)
        {
            this.value = value;
        }

        public override T Value
        {
            get
            {
                return value;
            }
        }
    }
}
