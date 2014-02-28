﻿namespace Sodium
{
    internal class SwitchEvent<TA> : Event<TA>
    {
        private SwitchEventAction<TA> eventSwitchCallback;
        private IEventListener<Event<TA>> listener;
        private Event<Event<TA>> updates;
        private Behavior<Event<TA>> behavior;

        public SwitchEvent(Transaction transaction, Behavior<Event<TA>> behavior, bool allowAutoDispose)
            : base(allowAutoDispose)
        {
            this.behavior = behavior;
            var action = new SodiumAction<TA>(this.Fire);
            this.eventSwitchCallback = new SwitchEventAction<TA>(behavior, this, transaction, action, true);
            this.updates = behavior.Updates();
            this.listener = updates.Listen(transaction, eventSwitchCallback, this.Rank, true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.listener != null)
                {
                    this.listener.AutoDispose();
                    this.listener = null;
                }

                if (eventSwitchCallback != null)
                {
                    eventSwitchCallback.AutoDispose();
                    eventSwitchCallback = null;
                }

                if (updates != null)
                {
                    updates.AutoDispose();
                    updates = null;
                }

                if (this.behavior != null)
                {
                    this.behavior.AutoDispose();
                    this.behavior = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
