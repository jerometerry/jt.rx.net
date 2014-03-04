﻿namespace Sodium
{
    using System;

    internal class BehaviorApplyEvent<T, TB> : EventSink<TB>
    {
        private Behavior<Func<T, TB>> bf;
        private Behavior<T> source;
        private IEventListener<Func<T, TB>> l1;
        private IEventListener<T> l2;
        
        /// <summary>
        /// Set to true when waiting for the Fire Priority Action to run.
        /// </summary>
        private bool fired;

        public BehaviorApplyEvent(Behavior<Func<T, TB>> bf, Behavior<T> source)
        {
            this.bf = bf;
            this.source = source;

            var functionChanged = new ActionCallback<Func<T, TB>>((f, t) => ScheduledPrioritizedFire(t));
            var valueChanged = new ActionCallback<T>((a, t) => ScheduledPrioritizedFire(t));

            l1 = bf.Listen(functionChanged, this.Rank);
            l2 = source.Listen(valueChanged, this.Rank);

            var map = bf.Value;
            var valA = source.Value;
            var valB = map(valA);
            this.Behavior = this.ToBehavior(valB);
        }

        public Behavior<TB> Behavior { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (this.Behavior != null)
            {
                this.Behavior.Dispose();
                this.Behavior = null;
            }

            if (l1 != null)
            {
                l1.Dispose();
                l1 = null;
            }

            if (l2 != null)
            {
                l2.Dispose();
                l2 = null;
            }

            bf = null;
            source = null;

            base.Dispose(disposing);
        }

        /// <summary>
        /// Schedule prioritized firing on the given transaction
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns>True if firing was added as a priority action on the given 
        /// transaction, false if there is already an scheduled firing that 
        /// is yet to fire.</returns>
        private bool ScheduledPrioritizedFire(Transaction transaction)
        {
            if (fired)
            {
                return false;
            }

            fired = true;
            transaction.High(Fire, this.Rank);
            return true;
        }

        private void Fire(Transaction transaction)
        {
            var b = this.GetNewValue();
            this.Fire(b, transaction);
            fired = false;
        }

        private TB GetNewValue()
        {
            var map = bf.GetNewValue();
            var a = source.GetNewValue();
            var b = map(a);
            return b;
        }
    }
}
