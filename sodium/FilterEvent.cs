namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class FilterEvent<T> : Event<T>
    {
        private Event<T> source;
        private Func<T, bool> f;
        private IEventListener<T> listener;

        public FilterEvent(Event<T> source, Func<T, bool> f)
        {
            this.source = source;
            this.f = f;

            var callback = new SodiumCallback<T>(this.Fire);
            this.listener = source.Listen(callback, this.Rank);
        }

        public override void Dispose()
        {
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            this.source = null;
            f = null;

            base.Dispose();
        }

        /// <summary>
        /// Fire the event if the predicate evaluates to true
        /// </summary>
        /// <param name="t"></param>
        /// <param name="a"></param>
        internal override void Fire(Transaction t, T a)
        {
            if (f(a))
            {
                base.Fire(t, a);
            }
        }

        protected internal override T[] InitialFirings()
        {
            var events = this.source.InitialFirings();
            if (events == null)
            {
                return null;
            }

            var filtered = events.Where(e => f(e)).ToList();
            if (!filtered.Any())
            {
                return null;
            }

            return filtered.ToArray();
        }
    }
}