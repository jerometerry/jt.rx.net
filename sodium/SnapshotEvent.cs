namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class SnapshotEvent<T, TB, TC> : SubscribeFireEvent<TC>
    {
        private IObservable<T> source;
        private Func<T, TB, TC> snapshot;
        private IValue<TB> behavior;
        private ISubscription<T> subscription;

        public SnapshotEvent(IObservable<T> source, Func<T, TB, TC> snapshot, IValue<TB> behavior)
        {
            this.source = source;
            this.snapshot = snapshot;
            this.behavior = behavior;

            var callback = new SodiumCallback<T>(this.Fire);
            this.subscription = source.Subscribe(callback, this.Rank);
        }

        public void Fire(T firing, Transaction transaction)
        {
            var f = this.behavior.Value;
            var v = this.snapshot(firing, f);
            this.Fire(v, transaction);
        }

        public override TC[] SubscriptionFirings()
        {
            var events = GetSubscribeFirings(source);
            if (events == null)
            {
                return null;
            }
            
            var results = events.Select(e => this.snapshot(e, this.behavior.Value));
            return results.ToArray();
        }

        protected override void Dispose(bool disposing)
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }

            source = null;
            behavior = null;
            snapshot = null;

            base.Dispose(disposing);
        }
    }
}