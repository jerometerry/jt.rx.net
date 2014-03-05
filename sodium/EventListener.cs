namespace Sodium
{
    internal sealed class EventListener<T> : DisposableObject, IEventListener<T>
    {
        public EventListener(Event<T> source, ISodiumCallback<T> callback, Rank rank)
        {
            this.Source = source;
            this.Callback = callback;
            this.Rank = rank;
        }

        public Event<T> Source { get; private set; }

        public ISodiumCallback<T> Callback { get; private set; }

        public Rank Rank { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (this.Source != null)
            {
                this.Source.RemoveListener(this);
                this.Source = null;
            }

            Callback = null;
            Rank = null;

            base.Dispose(disposing);
        }
    }
}