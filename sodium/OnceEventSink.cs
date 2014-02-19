﻿namespace sodium
{
    using System;

    class OnceEventSink<TEvent> : EventSink<TEvent>
    {
        private readonly Event<TEvent> _event;
        private readonly IListener[] _listeners;

        public OnceEventSink(Event<TEvent> ev, IListener[] listeners)
        {
            _event = ev;
            _listeners = listeners;
        }

        internal override TEvent[] SampleNow()
        {
            var inputs = _event.SampleNow();
            var outputs = inputs;
            if (outputs != null)
            {
                if (outputs.Length > 1)
                { 
                    outputs = new[] { inputs[0] };
                }

                if (_listeners[0] != null)
                {
                    _listeners[0].Unlisten();
                    _listeners[0] = null;
                }
            }
            return outputs;
        }
    }
}
