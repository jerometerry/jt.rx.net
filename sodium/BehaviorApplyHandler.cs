namespace Sodium
{
    using System;

    internal sealed class BehaviorApplyHandler<TA, TB> 
    {
        private readonly Event<TB> evt;
        private readonly Behavior<Func<TA, TB>> bf;
        private readonly Behavior<TA> ba;
        private bool fired;

        public BehaviorApplyHandler(Event<TB> evt, Behavior<Func<TA, TB>> bf, Behavior<TA> ba)
        {
            this.evt = evt;
            this.bf = bf;
            this.ba = ba;
        }

        public void Run(Transaction t1)
        {
            if (fired)
            { 
                return;
            }

            fired = true;
            t1.Prioritize(Fire, evt.Rank);
        }

        private void Fire(Transaction t)
        {
            var map = bf.NewValue();
            var a = ba.NewValue();
            var b = map(a);
            evt.Fire(t, b);
            fired = false;
        }
    }
}