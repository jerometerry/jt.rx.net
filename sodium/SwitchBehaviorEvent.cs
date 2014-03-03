﻿namespace Sodium
{
    internal class SwitchBehaviorEvent<T> : EventSink<T>
    {
        private IEventListener<Behavior<T>> listener;
        private IEventListener<T> eventListener;
        private Behavior<Behavior<T>> source;
        private Event<T> wrappedEvent;
        private Event<Behavior<T>> sourceEvent;

        public SwitchBehaviorEvent(Behavior<Behavior<T>> source)
        {
            this.source = source;
            this.sourceEvent = source.Values();
            var callback = new ActionCallback<Behavior<T>>(this.Invoke);
            this.listener = this.sourceEvent.Listen(callback, this.Rank);
        }

        public void Invoke(Behavior<T> behavior, Transaction transaction)
        {
            // Note: If any switch takes place during a transaction, then the
            // GetValueStream().Listen will always cause a sample to be fetched from the
            // one we just switched to. The caller will be fetching our output
            // using GetValueStream().Listen, and GetValueStream() throws away all firings except
            // for the last one. Therefore, anything from the old input behavior
            // that might have happened during this transaction will be suppressed.
            if (this.eventListener != null)
            {
                this.eventListener.Dispose();
                this.eventListener = null;
            }

            if (this.wrappedEvent != null)
            {
                this.wrappedEvent.Dispose();
                this.wrappedEvent = null;
            }

            this.wrappedEvent = behavior.Values(transaction);
            this.eventListener = wrappedEvent.Listen(this.CreateFireCallback(), Rank, transaction);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.listener != null)
            {
                this.listener.Dispose();
                this.listener = null;
            }

            if (this.eventListener != null)
            {
                this.eventListener.Dispose();
                this.eventListener = null;
            }

            if (this.wrappedEvent != null)
            {
                this.wrappedEvent.Dispose();
                this.wrappedEvent = null;
            }

            if (this.sourceEvent != null)
            {
                this.sourceEvent.Dispose();
                this.sourceEvent = null;
            }

            this.source = null;

            base.Dispose(disposing);
        }
    }
}
