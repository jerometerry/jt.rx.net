namespace Sodium
{
    using System.Linq;

    internal sealed class MergeEvent<T> : EventSink<T>
    {
        private Event<T> source1;
        private Event<T> source2;
        private IEventListener<T> l1;
        private IEventListener<T> l2;

        public MergeEvent(Event<T> source1, Event<T> source2)
        {
            this.source1 = source1;
            this.source2 = source2;

            var callback = this.CreateFireCallback();
            l1 = source1.Listen(callback, this.Rank);
            l2 = source2.Listen(callback, this.Rank);
        }

        public override void Dispose()
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

            base.Dispose();
        }

        protected internal override T[] InitialFirings()
        {
            var firings1 = source1.InitialFirings();
            var firings2 = source2.InitialFirings();

            if (firings1 != null && firings2 != null)
            {
                return firings1.Concat(firings2).ToArray();
            }

            return firings1 ?? firings2;
        }
    }
}