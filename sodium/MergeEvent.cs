namespace Sodium
{
    using System.Linq;

    internal sealed class MergeEvent<T> : EventSink<T>
    {
        private Event<T> source1;
        private Event<T> source2;
        private ISubscription<T> l1;
        private ISubscription<T> l2;

        public MergeEvent(Event<T> source1, Event<T> source2)
        {
            this.source1 = source1;
            this.source2 = source2;

            var callback = this.CreateFireCallback();
            l1 = source1.Subscribe(callback, this.Rank);
            l2 = source2.Subscribe(callback, this.Rank);
        }

        protected internal override T[] InitialFirings()
        {
            var firings1 = GetInitialFirings(source1);
            var firings2 = GetInitialFirings(source2);

            if (firings1 != null && firings2 != null)
            {
                return firings1.Concat(firings2).ToArray();
            }

            return firings1 ?? firings2;
        }

        protected override void Dispose(bool disposing)
        {
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

            source1 = null;
            source2 = null;

            base.Dispose(disposing);
        }
    }
}