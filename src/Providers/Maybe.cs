﻿namespace Potassium.Providers
{
    using System;

    /// <summary>
    /// Maybe is a class that might contain a value
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <remarks>
    /// The Maybe Monad http://en.wikibooks.org/wiki/Haskell/Understanding_monads/Maybe
    /// </remarks>
    public sealed class Maybe<T> : Provider<T>
    {
        private readonly T value;
        private readonly bool hasValue;

        /// <summary>
        /// Constructs a Maybe having no value
        /// </summary>
        public Maybe()
        {
            hasValue = false;
        }

        /// <summary>
        /// Constructs a Maybe with the given value
        /// </summary>
        /// <param name="value">The value to be set on the Maybe</param>
        public Maybe(T value)
        {
            this.value = value;
            this.hasValue = true;
        }

        /// <summary>
        /// A Maybe object without a value
        /// </summary>
        public static Maybe<T> Nothing
        {
            get
            {
                return new Maybe<T>();
            }
        }

        /// <summary>
        /// Gets whether the current Maybe object has a value
        /// </summary>
        public bool HasValue
        {
            get
            {
                return hasValue;
            }
        }

        /// <summary>
        /// Gets the value of the current Maybe
        /// </summary>
        /// <returns>The value of the current Maybe. If the Maybe has no value,
        /// an Exception will be raised.</returns>
        public override T Value
        {
            get
            {
                if (!HasValue)
                {
                    throw new InvalidOperationException("Maybe doesn't contain a value!");
                }

                return value;
            }
        }

        /// <summary>
        /// Implicitly convert a Maybe to its underlying value
        /// </summary>
        /// <param name="m">The Maybe object to convert</param>
        /// <returns>The underlying value</returns>
        /// <remarks>An exception will be raised if the Maybe has no value</remarks>
        public static implicit operator T(Maybe<T> m)
        {
            return m.Value;
        }

        /// <summary>
        /// Implicitly wrap a raw value into Maybe
        /// </summary>
        /// <param name="m">The raw value to wrap with a Maybe</param>
        /// <returns>A Maybe containing the specified value</returns>
        public static implicit operator Maybe<T>(T m)
        {
            return new Maybe<T>(m);
        }

        /// <summary>
        /// Convert the Maybe to a string simply by calling ToString on the underlying value.
        /// </summary>
        /// <returns>ToString of the underlying value if there is one, String.Empty otherwise.</returns>
        public override string ToString()
        {
            if (HasValue)
            {
                return value.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Compare equality of two Maybe objects, by simply comparing the underlying values
        /// </summary>
        /// <param name="obj">The object to compare to the current Maybe</param>
        /// <returns>True if the given object is a Maybe having the same underlying value, false otherwise</returns>
        public override bool Equals(object obj)
        {
            var m = obj as Maybe<T>;
            if (m == null)
            {
                return false;
            }

            if (HasValue != m.HasValue)
            { 
                return false;
            }

            return Value.Equals(m.Value);
        }

        /// <summary>
        /// Gets the hash code of the current Maybe
        /// </summary>
        /// <returns>The hash code of the underlying value if there is one, zero otherwise.</returns>
        public override int GetHashCode()
        {
            return !HasValue ? 0 : Value.GetHashCode();
        }
    }
}
