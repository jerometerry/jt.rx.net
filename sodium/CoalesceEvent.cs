namespace Sodium
{
    using System;

    internal sealed class CoalesceEvent<T> : Event<T>
    {
        private Event<T> source;
        private Func<T, T, T> coalesce;
        private IEventListener<T> listener;
        private Maybe<T> accumulatedValue = Maybe<T>.Null;

        public CoalesceEvent(Event<T> source, Func<T, T, T> coalesce, Transaction transaction)
        {
            this.source = source;
            this.coalesce = coalesce;

            var callback = new ActionCallback<T>(this.Accumulate);
            this.listener = source.Listen(transaction, callback, this.Rank);
        }

        public override void Dispose()
        {
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            source = null;
            coalesce = null;

            base.Dispose();
        }

        protected internal override T[] InitialFirings()
        {
            var events = source.InitialFirings();
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

        private void Accumulate(T data, Transaction transaction)
        {
            if (this.accumulatedValue.HasValue)
            {
                this.accumulatedValue = new Maybe<T>(coalesce(this.accumulatedValue.Value(), data));
            }
            else
            {
                transaction.Priority(Fire, this.Rank);
                this.accumulatedValue = new Maybe<T>(data);
            }
        }

        private void Fire(Transaction transaction)
        {
            var v = this.accumulatedValue.Value();
            this.Fire(v, transaction);
            this.accumulatedValue = Maybe<T>.Null;
        }
    }
}