namespace sodium
{
    sealed class EventSinkSender<TEvent> : ITransactionHandler<TEvent>
    {
        private readonly EventSink<TEvent> _sink;

        public EventSinkSender(EventSink<TEvent> sink)
        {
            _sink = sink;
        }

        public void Run(Transaction transaction, TEvent evt)
        {
            _sink.Send(transaction, evt);
        }

        public void Dispose()
        {
        }
    }
}