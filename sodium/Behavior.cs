namespace sodium
{
    using System;

    public class Behavior<A>
    {
        protected Event<A> _event;
        protected A _value;
        Maybe<A> valueUpdate = Maybe<A>.Null;
        private IListener cleanup;

        ///
        /// A behavior with a constant value.
        ///
        public Behavior(A value)
        {
            this._event = new Event<A>();
            this._value = value;
        }

        internal Behavior(Event<A> evt, A initValue)
        {
            this._event = evt;
            this._value = initValue;
            Behavior<A> thiz = this;

            Transaction.Run(new HandlerImpl<Transaction>(t1 =>
            {
                var handler = new TransactionHandler<A>((t2, a) =>
                {
                    if (!thiz.valueUpdate.HasValue)
                    {
                        t2.Last(new Runnable(() =>
                        {
                            thiz._value = thiz.valueUpdate.Value();
                            thiz.valueUpdate = Maybe<A>.Null;
                        }));
                    }
                    this.valueUpdate = new Maybe<A>(a);

                });
                this.cleanup = evt.listen(Node.Null, t1, handler, false);
            }));
        }

        ///
        /// @return The value including any updates that have happened in this transaction.
        ///
        A newValue()
        {
            return !valueUpdate.HasValue ? _value : valueUpdate.Value();
        }

        ///
        /// Sample the behavior's current value.
        ///
        /// This should generally be avoided in favour of value().listen(..) so you don't
        /// miss any updates, but in many circumstances it makes sense.
        ///
        /// It can be best to use it inside an explicit transaction (using Transaction.run()).
        /// For example, a b.sample() inside an explicit transaction along with a
        /// b.updates().listen(..) will capture the current value and any updates without risk
        /// of missing any in between.
        ///
        public A sample()
        {
            // Since pointers in Java are atomic, we don't need to explicitly create a
            // transaction.
            return _value;
        }

        ///
        /// An event that gives the updates for the behavior. If this behavior was created
        /// with a hold, then updates() gives you an event equivalent to the one that was held.
        ///
        public Event<A> updates()
        {
            return _event;
        }

        ///
        /// An event that is guaranteed to fire once when you listen to it, giving
        /// the current value of the behavior, and thereafter behaves like updates(),
        /// firing for each update to the behavior's value.
        ///
        public Event<A> value()
        {
            return Transaction.Apply(new Lambda1Impl<Transaction, Event<A>>(value));
        }

        Event<A> value(Transaction trans1)
        {
            var out_ = new ValueEventSink<A>(this);
            var l = _event.listen(out_.node, trans1,
                new TransactionHandler<A>(out_.send), false);
            return out_.addCleanup(l)
                .lastFiringOnly(trans1);  // Needed in case of an initial value and an update
            // in the same transaction.
        }

        private class ValueEventSink<A> : EventSink<A>
        {
            private Behavior<A> _behavior;

            public ValueEventSink(Behavior<A> behavior)
            {
                _behavior = behavior;
            }

            protected internal override Object[] sampleNow()
            {
                return new Object[] { _behavior.sample() };
            }
        }

        /// <summary>
        /// Overload of map that accepts a Func<A,B> to support C# lambdas
        /// </summary>
        /// <typeparam name="B"></typeparam>
        /// <param name="f"></param>
        /// <returns></returns>
        public Behavior<B> map<B>(Func<A, B> f)
        {
            return map(new Lambda1Impl<A, B>(f));
        }

        ///
        /// Transform the behavior's value according to the supplied function.
        ///
        public Behavior<B> map<B>(ILambda1<A, B> f)
        {
            return updates().map(f).hold(f.apply(sample()));
        }

        ///
        /// Lift a binary function into behaviors.
        ///
        public Behavior<C> lift<B, C>(ILambda2<A, B, C> f, Behavior<B> b)
        {
            ILambda1<A, ILambda1<B, C>> ffa = new Lambda1Impl<A, ILambda1<B, C>>((aa) =>
            {

                return new Lambda1Impl<B, C>((bb) =>
                                                 {
                                                     return f.apply(aa, bb);
                                                 });
            });
            Behavior<ILambda1<B, C>> bf = map(ffa);
            return apply(bf, b);
        }

        /// <summary>
        /// Overload of lift that accepts binary function Func<A,B,C> f and two behaviors, to enable C# lambdas
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        /// <typeparam name="C"></typeparam>
        /// <param name="f"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Behavior<C> lift<A, B, C>(Func<A, B, C> f, Behavior<A> a, Behavior<B> b)
        {
            return lift<A, B, C>(new Lambda2Impl<A, B, C>(f), a, b);
        }

        ///
        /// Lift a binary function into behaviors.
        ///
        public static Behavior<C> lift<A, B, C>(ILambda2<A, B, C> f, Behavior<A> a, Behavior<B> b)
        {
            return a.lift(f, b);
        }

        ///
        /// Lift a ternary function into behaviors.
        ///
        // TODO
        //public Behavior<D> lift<B, C, D>(Lambda3<A, B, C, D> f, Behavior<B> b, Behavior<C> c)
        //{
        //    
        //    Lambda1<A, Lambda1<B, Lambda1<C, D>>> ffa = null;
        //    //Lambda1<A, Lambda1<B, Lambda1<C,D>>> ffa = new Lambda1<A, Lambda1<B, Lambda1<C,D>>>() {
        //    //    public Lambda1<B, Lambda1<C,D>> apply(final A aa) {
        //    //        return new Lambda1<B, Lambda1<C,D>>() {
        //    //            public Lambda1<C,D> apply(final B bb) {
        //    //                return new Lambda1<C,D>() {
        //    //                    public D apply(C cc) {
        //    //                        return f.apply(aa,bb,cc);
        //    //                    }
        //    //                };
        //    //            }
        //    //        };
        //    //    }
        //    //};
        //    Behavior<Lambda1<B, Lambda1<C, D>>> bf = map(ffa);
        //    return apply(apply(bf, b), c);
        //}

        ///
        /// Lift a ternary function into behaviors.
        ///
        // TODO
        //public static Behavior<D> lift<A, B, C, D>(Lambda3<A, B, C, D> f, Behavior<A> a, Behavior<B> b, Behavior<C> c)
        //{
        //    return a.lift(f, b, c);
        //}

        ///
        /// Apply a value inside a behavior to a function inside a behavior. This is the
        /// primitive for all function lifting.
        ///
        public static Behavior<B> apply<A, B>(Behavior<ILambda1<A, B>> bf, Behavior<A> ba)
        {
            var out_ = new EventSink<B>();
            IHandler<Transaction> h = new ApplyHandler<A, B>(out_, bf, ba);
            var l1 = bf.updates().listen_(out_.node, new TransactionHandler<ILambda1<A, B>>((t, f) => h.run(t)));
            var l2 = ba.updates().listen_(out_.node, new TransactionHandler<A>((t, a) => h.run(t)));
            return out_.addCleanup(l1).addCleanup(l2).hold(bf.sample().apply(ba.sample()));
        }

        private class ApplyHandler<A, B> : IHandler<Transaction>
        {
            private bool fired = false;
            private EventSink<B> out_;
            private Behavior<ILambda1<A, B>> bf;
            private Behavior<A> ba;

            public ApplyHandler(EventSink<B> ev, Behavior<ILambda1<A, B>> bf, Behavior<A> ba)
            {
                out_ = ev;
                this.bf = bf;
                this.ba = ba;
            }

            public void run(Transaction trans1)
            {
                if (fired)
                    return;

                fired = true;
                trans1.Prioritized(out_.node, new HandlerImpl<Transaction>(t2 =>
                {
                    var v = bf.newValue();
                    var nv = ba.newValue();
                    var b = v.apply(nv);
                    out_.send(t2, b);
                    fired = false;
                }));
            }
        }

        ///
        /// Unwrap a behavior inside another behavior to give a time-varying behavior implementation.
        ///
        public static Behavior<A> switchB<A>(Behavior<Behavior<A>> bba)
        {
            A za = bba.sample().sample();
            var out_ = new EventSink<A>();
            var h = new SwitchHandler<A>(out_);
            var l1 = bba.value().listen_(out_.node, h);
            return out_.addCleanup(l1).hold(za);
        }

        private class SwitchHandler<A> : ITransactionHandler<Behavior<A>>
        {
            private IListener currentListener;
            private EventSink<A> out_;

            public SwitchHandler(EventSink<A> o)
            {
                out_ = o;
            }

            public void Run(Transaction trans2, Behavior<A> ba)
            {
                // Note: If any switch takes place during a transaction, then the
                // value().listen will always cause a sample to be fetched from the
                // one we just switched to. The caller will be fetching our output
                // using value().listen, and value() throws away all firings except
                // for the last one. Therefore, anything from the old input behaviour
                // that might have happened during this transaction will be suppressed.
                if (currentListener != null)
                    currentListener.unlisten();

                Event<A> ev = ba.value(trans2);
                currentListener = ev.listen(out_.node, trans2, new TransactionHandler<A>(Handler), false);
            }

            private void Handler(Transaction t3, A a)
            {
                out_.send(t3, a);
            }

            ~SwitchHandler()
            {
                if (currentListener != null)
                    currentListener.unlisten();
            }
        }

        ///
        /// Unwrap an event inside a behavior to give a time-varying event implementation.
        ///
        public static Event<A> switchE<A>(Behavior<Event<A>> bea)
        {
            return Transaction.Apply(new Lambda1Impl<Transaction, Event<A>>((t) => switchE<A>(t, bea)));
        }

        private static Event<A> switchE<A>(Transaction trans1, Behavior<Event<A>> bea)
        {
            var out_ = new EventSink<A>();
            var h2 = new TransactionHandler<A>(out_.send);

            var h1 = new SwitchEHandler<A>(bea, out_, trans1, h2);
            var l1 = bea.updates().listen(out_.node, trans1, h1, false);
            return out_.addCleanup(l1);
        }

        private class SwitchEHandler<A> : ITransactionHandler<Event<A>>
        {
            private IListener currentListener;
            private EventSink<A> out_;
            private Transaction trans1;
            private ITransactionHandler<A> h2;

            public SwitchEHandler(Behavior<Event<A>> bea, EventSink<A> out_, Transaction trans1, ITransactionHandler<A> h2)
            {
                this.out_ = out_;
                this.trans1 = trans1;
                this.h2 = h2;
                currentListener = bea.sample().listen(out_.node, trans1, h2, false);
            }

            public void Run(Transaction trans2, Event<A> ea)
            {
                trans2.Last(new Runnable(() =>
                {
                    if (currentListener != null)
                        currentListener.unlisten();
                    currentListener = ea.listen(out_.node, trans2, h2, true);
                }));
            }

            ~SwitchEHandler()
            {
                if (currentListener != null)
                    currentListener.unlisten();
            }
        }

        ///
        /// Transform a behavior with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        ///
        public Behavior<B> collect<B, S>(S initState, ILambda2<A, S, Tuple2<B, S>> f)
        {
            Event<A> ea = updates().coalesce(new Lambda2Impl<A, A, A>((a, b) => b));
            A za = sample();
            Tuple2<B, S> zbs = f.apply(za, initState);
            EventLoop<Tuple2<B, S>> ebs = new EventLoop<Tuple2<B, S>>();
            Behavior<Tuple2<B, S>> bbs = ebs.hold(zbs);
            Behavior<S> bs = bbs.map(new Lambda1Impl<Tuple2<B, S>, S>(x => x.V2));
            Event<Tuple2<B, S>> ebs_out = ea.snapshot(bs, f);
            ebs.loop(ebs_out);
            return bbs.map(new Lambda1Impl<Tuple2<B, S>, B>(x => x.V1));
        }

        ~Behavior()
        {
            if (cleanup != null)
                cleanup.unlisten();
        }

    }
}