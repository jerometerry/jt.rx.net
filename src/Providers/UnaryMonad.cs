﻿namespace Potassium.Providers
{
    using System;
    using Potassium.Core;
    using Potassium.Providers;

    /// <summary>
    /// A UnaryMonad lifts a unary function into a Monad
    /// </summary>
    /// <typeparam name="T">The type of first parameter to the lift function</typeparam>
    /// <typeparam name="TB">The return type of the lift function</typeparam>
    public class UnaryMonad<T, TB> : Provider<TB>
    {
        private Func<T, TB> lift;
        private IProvider<T> source;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lift"></param>
        /// <param name="source"></param>
        public UnaryMonad(Func<T, TB> lift, IProvider<T> source)
        {
            this.lift = lift;
            this.source = source;
        }

        public override TB Value
        {
            get
            {
                return lift(source.Value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lift = null;
                source = null;
            }

            base.Dispose(disposing);
        }
    }
}
