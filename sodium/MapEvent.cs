﻿namespace Sodium
{
    using System;
    using System.Linq;

    internal sealed class MapEvent<T, TB> : EventSink<TB>
    {
        private Event<T> source;
        private Func<T, TB> map;
        private ISubscription<T> subscription;

        public MapEvent(Event<T> source, Func<T, TB> map)
        {
            this.source = source;
            this.map = map;
            this.subscription = source.Subscribe(new ActionCallback<T>(this.Fire), this.Rank);
        }

        public void Fire(T firing, Transaction trans)
        {
            var v = this.map(firing);
            this.Fire(v, trans);
        }

        protected internal override TB[] InitialFirings()
        {
            var firings = GetInitialFirings(source);
            if (firings == null)
            { 
                return null;
            }

            return firings.Select(e => map(e)).ToArray();
        }

        protected override void Dispose(bool disposing)
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }

            source = null;
            map = null;

            base.Dispose(disposing);
        }
    }
}
