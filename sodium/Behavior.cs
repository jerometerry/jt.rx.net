namespace sodium
{
    using System;

    public class Behavior<TA>
    {
        protected Event<TA> Event;
        protected TA Val;
        Maybe<TA> _valueUpdate = Maybe<TA>.Null;
        private IListener _cleanup;

        ///
        /// A behavior with a constant value.
        ///
        public Behavior(TA val)
        {
            Event = new Event<TA>();
            Val = val;
        }

        internal Behavior(Event<TA> evt, TA initVal)
        {
            Event = evt;
            Val = initVal;
            var behavior = this;

            Transaction.Run(new HandlerImpl<Transaction>(t1 =>
            {
                var handler = new TransactionHandler<TA>((t2, a) =>
                {
                    if (!behavior._valueUpdate.HasValue)
                    {
                        t2.Last(new Runnable(() =>
                        {
                            behavior.Val = behavior._valueUpdate.Value();
                            behavior._valueUpdate = Maybe<TA>.Null;
                        }));
                    }
                    _valueUpdate = new Maybe<TA>(a);

                });
                _cleanup = evt.Listen(Node.Null, t1, handler, false);
            }));
        }

        ///
        /// @return The value including any updates that have happened in this transaction.
        ///
        internal TA NewValue()
        {
            return !_valueUpdate.HasValue ? Val : _valueUpdate.Value();
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
        public TA Sample()
        {
            // Since pointers in Java are atomic, we don't need to explicitly create a
            // transaction.
            return Val;
        }

        ///
        /// An event that gives the updates for the behavior. If this behavior was created
        /// with a hold, then updates() gives you an event equivalent to the one that was held.
        ///
        public Event<TA> Updates()
        {
            return Event;
        }

        ///
        /// An event that is guaranteed to fire once when you listen to it, giving
        /// the current value of the behavior, and thereafter behaves like updates(),
        /// firing for each update to the behavior's value.
        ///
        public Event<TA> Value()
        {
            return Transaction.Apply(new Lambda1Impl<Transaction, Event<TA>>(Value));
        }

        internal Event<TA> Value(Transaction trans1)
        {
            var sink = new BehaviorValueEventSink<TA>(this);
            var l = Event.Listen(sink.Node, trans1,
                new TransactionHandler<TA>(sink.send), false);
            return sink.RegisterListener(l)
                .LastFiringOnly(trans1);  // Needed in case of an initial value and an update
                                          // in the same transaction.
        }

        /// <summary>
        /// Overload of map that accepts a Func to support C# lambdas
        /// </summary>
        /// <typeparam name="TB"></typeparam>
        /// <param name="f"></param>
        /// <returns></returns>
        public Behavior<TB> Map<TB>(Func<TA, TB> f)
        {
            return Map(new Lambda1Impl<TA, TB>(f));
        }

        ///
        /// Transform the behavior's value according to the supplied function.
        ///
        public Behavior<TB> Map<TB>(ILambda1<TA, TB> f)
        {
            return Updates().Map(f).Hold(f.apply(Sample()));
        }

        ///
        /// Lift a binary function into behaviors.
        ///
        public Behavior<TC> Lift<TB, TC>(ILambda2<TA, TB, TC> f, Behavior<TB> b)
        {
            var ffa = new Lambda1Impl<TA, ILambda1<TB, TC>>(aa => new Lambda1Impl<TB, TC>(bb => f.apply(aa, bb)));
            var bf = Map(ffa);
            return Behavior<TB>.Apply(bf, b);
        }

        /// <summary>
        /// Overload of lift that accepts binary function Func f and two behaviors, to enable C# lambdas
        /// </summary>
        /// <param name="f"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Behavior<TC> Lift<TB, TC>(Func<TA, TB, TC> f, Behavior<TA> a, Behavior<TB> b)
        {
            return Lift(new Lambda2Impl<TA, TB, TC>(f), a, b);
        }

        ///
        /// Lift a binary function into behaviors.
        ///
        public static Behavior<TC> Lift<TB, TC>(ILambda2<TA, TB, TC> f, Behavior<TA> a, Behavior<TB> b)
        {
            return a.Lift(f, b);
        }

        ///
        /// Lift a ternary function into behaviors.
        ///
        // TODO
        //public Behavior<D> Lift<B, C, D>(Lambda3<TA, B, C, D> f, Behavior<B> b, Behavior<C> c)
        //{
        //    
        //    Lambda1<TA, Lambda1<B, Lambda1<C, D>>> ffa = null;
        //    //Lambda1<TA, Lambda1<B, Lambda1<C,D>>> ffa = new Lambda1<TA, Lambda1<B, Lambda1<C,D>>>() {
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
        //public static Behavior<D> Lift<TA, B, C, D>(Lambda3<TA, B, C, D> f, Behavior<TA> a, Behavior<B> b, Behavior<C> c)
        //{
        //    return a.lift(f, b, c);
        //}

        ///
        /// Apply a value inside a behavior to a function inside a behavior. This is the
        /// primitive for all function lifting.
        ///
        public static Behavior<TB> Apply<TB>(Behavior<ILambda1<TA, TB>> bf, Behavior<TA> ba)
        {
            var sink = new EventSink<TB>();
            var h = new BehaviorApplyHandler<TA, TB>(sink, bf, ba);
            var l1 = bf.Updates().Listen(sink.Node, new TransactionHandler<ILambda1<TA, TB>>((t, f) => h.run(t)));
            var l2 = ba.Updates().Listen(sink.Node, new TransactionHandler<TA>((t, a) => h.run(t)));
            return sink.RegisterListener(l1).RegisterListener(l2).Hold(bf.Sample().apply(ba.Sample()));
        }

        ///
        /// Unwrap a behavior inside another behavior to give a time-varying behavior implementation.
        ///
        public static Behavior<TA> SwitchB(Behavior<Behavior<TA>> bba)
        {
            var za = bba.Sample().Sample();
            var sink = new EventSink<TA>();
            var h = new BehaviorSwitchHandler<TA>(sink);
            var l1 = bba.Value().Listen(sink.Node, h);
            return sink.RegisterListener(l1).Hold(za);
        }

        ///
        /// Unwrap an event inside a behavior to give a time-varying event implementation.
        ///
        public static Event<TA> SwitchE(Behavior<Event<TA>> bea)
        {
            return Transaction.Apply(new Lambda1Impl<Transaction, Event<TA>>(t => SwitchE(t, bea)));
        }

        private static Event<TA> SwitchE(Transaction trans1, Behavior<Event<TA>> bea)
        {
            var sink = new EventSink<TA>();
            var h2 = new TransactionHandler<TA>(sink.send);
            var h1 = new EventSwitchHandler<TA>(bea, sink, trans1, h2);
            var l1 = bea.Updates().Listen(sink.Node, trans1, h1, false);
            return sink.RegisterListener(l1);
        }

        ///
        /// Transform a behavior with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        ///
        public Behavior<TB> Collect<TB, TS>(TS initState, ILambda2<TA, TS, Tuple2<TB, TS>> f)
        {
            var ea = Updates().Coalesce(new Lambda2Impl<TA, TA, TA>((a, b) => b));
            var za = Sample();
            var zbs = f.apply(za, initState);
            var ebs = new EventLoop<Tuple2<TB, TS>>();
            var bbs = ebs.Hold(zbs);
            var bs = bbs.Map(new Lambda1Impl<Tuple2<TB, TS>, TS>(x => x.V2));
            var ebsOut = ea.Snapshot(bs, f);
            ebs.Loop(ebsOut);
            return bbs.Map(new Lambda1Impl<Tuple2<TB, TS>, TB>(x => x.V1));
        }

        ~Behavior()
        {
            if (_cleanup != null)
                _cleanup.unlisten();
        }

    }
}