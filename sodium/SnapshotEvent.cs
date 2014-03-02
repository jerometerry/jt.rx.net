namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class SnapshotEvent<T, TB, TC> : Event<TC>
    {
        private Event<T> source;
        private Func<T, TB, TC> snapshot;
        private Behavior<TB> behavior;
        private IEventListener<T> listener;

        public SnapshotEvent(Event<T> source, Func<T, TB, TC> snapshot, Behavior<TB> behavior)
        {
            this.source = source;
            this.snapshot = snapshot;
            this.behavior = behavior;

            var callback = new SodiumCallback<T>(this.Fire);
            this.listener = source.Listen(callback, this.Rank);
        }

        public void Fire(Transaction transaction, T firing)
        {
            this.Fire(transaction, this.snapshot(firing, this.behavior.Sample()));
        }

        public override void Dispose()
        {
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            source = null;
            behavior = null;
            snapshot = null;

            base.Dispose();
        }

        protected internal override TC[] InitialFirings()
        {
            var events = source.InitialFirings();
            if (events == null)
            {
                return null;
            }
            
            var results = events.Select(e => this.snapshot(e, this.behavior.Sample()));
            return results.ToArray();
        }
    }
}