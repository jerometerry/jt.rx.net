namespace Sodium
{
    using System;

    internal sealed class CoalesceEvent<TA> : Event<TA>
    {
        private readonly Event<TA> evt;
        private readonly Func<TA, TA, TA> coalesce;
        private IEventListener<TA> listener;

        public CoalesceEvent(Event<TA> evt, Func<TA, TA, TA> coalesce, Transaction transaction)
        {
            this.evt = evt;
            this.coalesce = coalesce;

            var action = new CoalesceAction<TA>(this, coalesce);
            this.listener = evt.Listen(transaction, action, this.Rank);
        }

        protected internal override TA[] InitialFirings()
        {
            var events = evt.InitialFirings();
            if (events == null)
            {
                return null;
            }
            
            var e = events[0];
            for (var i = 1; i < events.Length; i++)
            {
                e = coalesce(e, events[i]);
            }

            return new[] { e };
        }

        protected override void Dispose(bool disposing)
        {
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            base.Dispose(disposing);
        }
    }
}