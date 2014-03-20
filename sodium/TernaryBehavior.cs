﻿namespace Sodium
{
    using System;

    /// <summary>
    /// TernaryBehavior is a continuous Behavior that lifts a ternary function into a Behavior.
    /// The value of the TernaryBehavior is computed by calling the ternary function, where
    /// the parameters are the current values of Behaviors.
    /// </summary>
    /// <typeparam name="T">The type of the first parameter to the ternary function</typeparam>
    /// <typeparam name="TB">The type of the second parameter to the ternary function</typeparam>
    /// <typeparam name="TC">They type of the third parameter to the ternary function</typeparam>
    /// <typeparam name="TD">The return type of the ternary function</typeparam>
    public class TernaryBehavior<T, TB, TC, TD> : ContinuousBehavior<TD>
    {
        private Func<T, TB, TC, TD> lift;
        private IBehavior<T> a;
        private IBehavior<TB> b;
        private IBehavior<TC> c;

        public TernaryBehavior(Func<T, TB, TC, TD> lift, IBehavior<T> a, IBehavior<TB> b, IBehavior<TC> c)
        {
            this.lift = lift;
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public override TD Value
        {
            get
            {
                return lift(a.Value, b.Value, c.Value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lift = null;
                a = null;
                b = null;
                c = null;
            }

            base.Dispose(disposing);
        }
    }
}
