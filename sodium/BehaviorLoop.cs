﻿namespace Sodium
{
    public sealed class BehaviorLoop<TA> : Behavior<TA>
    {
        public BehaviorLoop()
            : this(false)
        {
        }

        public BehaviorLoop(bool allowAutoDispose)
            : base(new EventLoop<TA>(true), default(TA), allowAutoDispose)
        {
        }

        /// <summary>
        /// Firings on b will be forwarded to the current Behavior
        /// </summary>
        /// <param name="b">Behavior who's firings will be looped to the current Behavior</param>
        /// <returns>The current Behavior</returns>
        /// <remarks>Loop can only be called once on a Behavior. If Loop is called multiple times,
        /// an ApplicationException will be raised.</remarks>
        public Behavior<TA> Loop(Behavior<TA> b)
        {
            var loop = (EventLoop<TA>)Event;
            loop.Loop(b.Updates());
            return this;
        }

        protected override void Dispose(bool disposing)
        {
            if (this.Event != null)
            {
                this.Event.AutoDispose();
            }

            base.Dispose(disposing);
        }
    }
}
