﻿namespace Potassium.Providers
{
    using System;
    using Potassium.Core;

    /// <summary>
    /// A BinaryBehavior lifts a binary function into a Monad
    /// </summary>
    /// <typeparam name="T">The type of first parameter to the lift function</typeparam>
    /// <typeparam name="TB">The type of the second parameter of the lift function</typeparam>
    /// <typeparam name="TC">The return type of the lift function</typeparam>
    public class BinaryMonad<T, TB, TC> : Provider<TC>
    {
        private Func<T, TB, TC> lift;
        private IProvider<T> a;
        private IProvider<TB> b;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lift"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public BinaryMonad(Func<T, TB, TC> lift, IProvider<T> a, IProvider<TB> b)
        {
            this.lift = lift;
            this.a = a;
            this.b = b;
        }

        public override TC Value
        {
            get
            {
                return lift(a.Value, b.Value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lift = null;
                a = null;
                b = null;
            }

            base.Dispose(disposing);
        }
    }
}
