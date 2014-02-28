namespace Sodium
{
    using System.Linq;

    internal sealed class MergeEvent<TA> : Event<TA>
    {
        private Event<TA> evt1;
        private Event<TA> evt2;
        private IEventListener<TA> l1;
        private IEventListener<TA> l2;

        public MergeEvent(Event<TA> evt1, Event<TA> evt2)
        {
            this.evt1 = evt1;
            this.evt2 = evt2;

            var action = new SodiumAction<TA>(this.Fire);
            l1 = evt1.Listen(action, this.Rank);
            l2 = evt2.Listen(action, this.Rank);
        }

        protected internal override TA[] InitialFirings()
        {
            var firings1 = evt1.InitialFirings();
            var firings2 = evt2.InitialFirings();

            if (firings1 != null && firings2 != null)
            {
                return firings1.Concat(firings2).ToArray();
            }

            return firings1 ?? firings2;
        }

        public override void Close()
        {
            if (l1 != null)
            {
                l1.Close();
                l1 = null;
            }

            if (l2 != null)
            {
                l2.Close();
                l2 = null;
            }

            if (evt1 != null)
            {
                evt1 = null;
            }

            if (evt2 != null)
            {
                evt2 = null;
            }

            base.Close();
        }
    }
}