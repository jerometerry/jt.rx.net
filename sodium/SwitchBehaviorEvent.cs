﻿namespace Sodium
{
    internal class SwitchBehaviorEvent<TA> : Event<TA>
    {
        private IEventListener<Behavior<TA>> listener;
        private IEventListener<TA> eventListener;
        private Behavior<Behavior<TA>> bba;
        private Event<TA> valueEvent; 

        public SwitchBehaviorEvent(Behavior<Behavior<TA>> bba, bool allowAutoDispose)
            : base(allowAutoDispose)
        {
            this.bba = bba;
            var action = new SodiumAction<Behavior<TA>>(this.Invoke);
            var v = bba.Value(true);
            this.RegisterAutoFinalizer(v);
            this.listener = v.Listen(action, this.Rank, true);
        }

        public void Invoke(Transaction transaction, Behavior<TA> behavior)
        {
            // Note: If any switch takes place during a transaction, then the
            // Value().Listen will always cause a sample to be fetched from the
            // one we just switched to. The caller will be fetching our output
            // using Value().Listen, and Value() throws away all firings except
            // for the last one. Therefore, anything from the old input behavior
            // that might have happened during this transaction will be suppressed.
            if (this.eventListener != null)
            {
                this.eventListener.AutoDispose();
                this.eventListener = null;
            }

            if (this.valueEvent != null)
            {
                this.valueEvent.AutoDispose();
                this.valueEvent = null;
            }

            this.valueEvent = behavior.Value(transaction, true);
            this.eventListener = valueEvent.Listen(transaction, new SodiumAction<TA>(Fire), Rank, true);
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

                if (this.eventListener != null)
                {
                    this.eventListener.AutoDispose();
                    this.eventListener = null;
                }

                if (this.bba != null)
                {
                    this.bba.AutoDispose();
                    this.bba = null;
                }

                if (this.valueEvent != null)
                {
                    this.valueEvent.AutoDispose();
                    this.valueEvent = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
