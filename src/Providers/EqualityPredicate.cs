﻿namespace Potassium.Providers
{
    /// <summary>
    /// EqualityPredicate is a Predicate that (lazily) determines if the value of an IProvider is equal to a given constant value
    /// </summary>
    /// <typeparam name="T">The underlying type of the Predicate</typeparam>
    /// <remarks>EqualityPredicate is lazy in that the equality check doesn't happen until the Value (bool) is requested.</remarks>
    public class EqualityPredicate<T> : Predicate
    {
        private IProvider<T> provider;
        private T v;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="v"></param>
        public EqualityPredicate(IProvider<T> provider, T v)
        {
            this.provider = provider;
            this.v = v;
        }

        /// <summary>
        /// Evaluates the value of the Provider
        /// </summary>
        public override bool Value
        {
            get
            {
                var v1 = new Maybe<T>(this.provider.Value);
                var v2 = new Maybe<T>(v);
                return v1.Equals(v2);
            }
        }

        /// <summary>
        /// Clean up all resources used by the current SodiumObject
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources
        /// </param>
        protected override void Dispose(bool disposing)
        {
            this.provider = null;
            v = default(T);

            base.Dispose(disposing);
        }
    }
}
