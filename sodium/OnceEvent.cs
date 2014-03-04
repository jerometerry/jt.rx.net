namespace Sodium
{
    internal sealed class OnceEvent<T> : EventSink<T>
    {
        private Event<T> source;
        private IEventListener<T>[] eventListeners;

        public OnceEvent(Event<T> source)
        {
            this.source = source;

            // This is a bit long-winded but it's efficient because it de-registers
            // the listener.
            this.eventListeners = new IEventListener<T>[1];
            this.eventListeners[0] = source.Listen(new ActionCallback<T>((a, t) => this.Fire(this.eventListeners, a, t)), this.Rank);
        }

        protected internal override T[] InitialFirings()
        {
            var firings = GetInitialFirings(this.source);
            if (firings == null)
            {
                return null;
            }

            var results = firings;
            if (results.Length > 1)
            { 
                results = new[] { firings[0] };
            }

            if (this.eventListeners[0] != null)
            {
                this.eventListeners[0].Dispose();
                this.eventListeners[0] = null;
            }

            return results;
        }

        protected override void Dispose(bool disposing)
        {
            if (this.eventListeners[0] != null)
            {
                this.eventListeners[0].Dispose();
                this.eventListeners[0] = null;
            }

            this.source = null;
            eventListeners = null;

            base.Dispose(disposing);
        }

        private void Fire(IEventListener<T>[] la, T a, Transaction t)
        {
            this.Send(a, t);
            if (la[0] == null)
            {
                return;
            }

            la[0].Dispose();
            la[0] = null;
        }
    }
}