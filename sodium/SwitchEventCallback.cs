namespace Sodium
{
    using System;

    internal sealed class SwitchEventCallback<TA> : ICallback<Event<TA>>, IDisposable
    {
        private readonly Event<TA> evt;
        private readonly ICallback<TA> action;
        private IListener<TA> listener;
        private bool disposed;

        public SwitchEventCallback(Behavior<Event<TA>> bea, Event<TA> evt, Transaction t, ICallback<TA> h)
        {
            this.evt = evt;
            this.action = h;
            this.listener = bea.Sample().Listen(t, h, evt.Rank);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            disposed = true;
        }

        public void Invoke(Transaction transaction, Event<TA> newEvent)
        {
            transaction.Last(() =>
            {
                if (listener != null)
                { 
                    listener.Dispose();
                }

                listener = newEvent.ListenSuppressed(transaction, this.action, evt.Rank);
            });
        }
    }
}