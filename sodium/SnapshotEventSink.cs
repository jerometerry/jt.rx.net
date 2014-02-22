namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class SnapshotEventSink<TA, TB, TC> : EventSink<TC>
    {
        private readonly Event<TA> evt;
        private readonly ILambda2<TA, TB, TC> f;
        private readonly Behavior<TB> b;

        public SnapshotEventSink(Event<TA> ev, ILambda2<TA, TB, TC> f, Behavior<TB> b)
        {
            this.evt = ev;
            this.f = f;
            this.b = b;
        }

        protected internal override TC[] SampleNow()
        {
            var events = evt.SampleNow();
            if (events == null)
            {
                return null;
            }
            
            var results = events.Select(e => f.Apply(e, b.Sample()));
            return results.ToArray();
        }
    }
}