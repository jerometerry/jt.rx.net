namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class SnapshotEvent<TA, TB, TC> : Event<TC>
    {
        private Event<TA> evt;
        private Func<TA, TB, TC> snapshot;
        private Behavior<TB> behavior;
        private IEventListener<TA> listener;

        public SnapshotEvent(Event<TA> ev, Func<TA, TB, TC> snapshot, Behavior<TB> behavior)
        {
            this.evt = ev;
            this.snapshot = snapshot;
            this.behavior = behavior;

            var action = new SodiumAction<TA>(this.Fire);
            this.listener = ev.Listen(action, this.Rank);
        }

        public void Fire(Transaction transaction, TA firing)
        {
            this.Fire(transaction, this.snapshot(firing, this.behavior.Sample()));
        }

        protected internal override TC[] InitialFirings()
        {
            var events = evt.InitialFirings();
            if (events == null)
            {
                return null;
            }
            
            var results = events.Select(e => this.snapshot(e, this.behavior.Sample()));
            return results.ToArray();
        }

        public override void Dispose()
        {
            if (listener != null)
            {
                listener = null;
            }

            if (evt != null)
            {
                evt = null;
            }

            if (behavior != null)
            {
                behavior = null;
            }

            snapshot = null;

            base.Dispose();
        }
    }
}