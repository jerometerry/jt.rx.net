namespace Sodium
{
    using System;

    internal sealed class OnceEventSink<TA> : EventSink<TA>
    {
        private readonly Event<TA> evt;
        private readonly IListener[] listeners;

        public OnceEventSink(Event<TA> evt, IListener[] listeners)
        {
            this.evt = evt;
            this.listeners = listeners;
        }

        public void Send(IListener[] la, Transaction t, TA a)
        {
            this.Send(t, a);
            if (la[0] == null)
            {
                return;
            }

            la[0].Unlisten();
            la[0] = null;
        }

        protected internal override TA[] SampleNow()
        {
            var events = evt.SampleNow();
            if (events == null)
            {
                return null;
            }

            var results = events;
            if (results.Length > 1)
            { 
                results = new[] { events[0] };
            }

            if (listeners[0] != null)
            {
                listeners[0].Unlisten();
                listeners[0] = null;
            }

            return results;
        }
    }
}