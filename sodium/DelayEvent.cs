﻿namespace Sodium
{
    internal class DelayEvent<T> : EventSink<T>
    {
        private IEventListener<T> listener;

        public DelayEvent(Event<T> source)
        {
            var callback = new ActionCallback<T>((a, t) => t.Low(() => this.Send(a)));
            this.listener = source.Listen(callback, this.Rank);
        }

        protected override void Dispose(bool disposing)
        {
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            base.Dispose(disposing);
        }
    }
}
