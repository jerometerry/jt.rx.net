namespace sodium
{
    using System;
    using System.Collections.Generic;

    public class Event<A>
    {
        private sealed class ListenerImplementation<A> : Listener
        {
            ///
            /// It's essential that we keep the listener alive while the caller holds
            /// the Listener, so that the finalizer doesn't get triggered.
            ///
            private readonly Event<A> _event;

            private readonly TransactionHandler<A> action;
            private readonly Node target;

            public ListenerImplementation(Event<A> evt, TransactionHandler<A> action, Node target)
            {
                this._event = evt;
                this.action = action;
                this.target = target;
            }

            public override void unlisten()
            {
                lock (Transaction.listenersLock)
                {
                    _event.listeners.Remove(action);
                    _event.node.unlinkTo(target);
                }
            }

            ~ListenerImplementation()
            {
                unlisten();
            }
        }


        protected List<TransactionHandler<A>> listeners = new List<TransactionHandler<A>>();
        protected List<Listener> finalizers = new List<Listener>();
        internal Node node = new Node(0L);
        protected List<A> firings = new List<A>();

        /// <summary>
        /// An event that never fires.
        /// </summary>
        public Event()
        {
        }

        protected internal virtual Object[] sampleNow()
        {
            return null;
        }

        /// <summary>
        /// Overload of the listen method that accepts and Action, to support C# lambda expressions
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Listener listen(Action<A> action)
        {
            return listen(new HandlerImpl<A>(action));
        }

        /// <summary>
        /// Listen for firings of this event. The returned Listener has an unlisten()
        /// method to cause the listener to be removed. This is the observer pattern.
        ///</summary>
        public Listener listen(Handler<A> action)
        {
            return listen_(Node.NULL, new TransactionHandlerImpl<A>((t, a) => action.run(a)));
        }

        internal Listener listen_(Node target, TransactionHandler<A> action)
        {
            return Transaction.apply(new Lambda1Impl<Transaction, Listener>(t => listen(target, t, action, false)));
        }

        internal Listener listen(Node target, Transaction trans, TransactionHandler<A> action,
                                bool suppressEarlierFirings)
        {
            lock (Transaction.listenersLock)
            {
                if (node.linkTo(target))
                    trans.toRegen = true;
                listeners.Add(action);
            }
            Object[] aNow = sampleNow();
            if (aNow != null)
            {
                // In cases like value(), we start with an initial value.
                for (int i = 0; i < aNow.Length; i++)
                    action.run(trans, (A)aNow[i]); // <-- unchecked warning is here
            }
            if (!suppressEarlierFirings)
            {
                // Anything sent already in this transaction must be sent now so that
                // there's no order dependency between send and listen.
                foreach (A a in firings)
                    action.run(trans, a);
            }
            return new ListenerImplementation<A>(this, action, target);
        }

        /// <summary>
        /// Overload of map that accepts a Func<A,B>, allowing for C# lambda support
        /// </summary>
        /// <typeparam name="B"></typeparam>
        /// <param name="f"></param>
        /// <returns></returns>
        public Event<B> map<B>(Func<A, B> f)
        {
            return map(new Lambda1Impl<A, B>(f));
        }

        ///
        /// Transform the event's value according to the supplied function.
        ///
        public Event<B> map<B>(Lambda1<A, B> f)
        {
            Event<A> ev = this;
            EventSink<B> out_ = new MapEventSink<A, B>(ev, f);
            Listener l = listen_(out_.node, new TransactionHandlerImpl<A>((t, a) => out_.send(t, f.apply(a))));
            return out_.addCleanup(l);
        }

        ///
        /// Create a behavior with the specified initial value, that gets updated
        /// by the values coming through the event. The 'current value' of the behavior
        /// is notionally the value as it was 'at the start of the transaction'.
        /// That is, state updates caused by event firings get processed at the end of
        /// the transaction.
        ///
        public Behavior<A> hold(A initValue)
        {
            return Transaction.apply(new Lambda1Impl<Transaction, Behavior<A>>(t => new Behavior<A>(lastFiringOnly(t), initValue)));
        }

        ///
        /// Variant of snapshot that throws away the event's value and captures the behavior's.
        ///
        public Event<B> snapshot<B>(Behavior<B> beh)
        {
            return snapshot(beh, new Lambda2Impl<A, B, B>((a, b) => b));
        }

        ///
        /// Sample the behavior at the time of the event firing. Note that the 'current value'
        /// of the behavior that's sampled is the value as at the start of the transaction
        /// before any state changes of the current transaction are applied through 'hold's.
        ///
        public Event<C> snapshot<B, C>(Behavior<B> b, Lambda2<A, B, C> f)
        {
            Event<A> ev = this;
            EventSink<C> out_ = new SnapshotEventSink<A, B, C>(ev, f, b);
            Listener l = listen_(out_.node, new TransactionHandlerImpl<A>((t2, a) => out_.send(t2, f.apply(a, b.sample()))));
            return out_.addCleanup(l);
        }

        private class SnapshotEventSink<A, B, C> : EventSink<C>
        {
            private Event<A> ev;
            private Lambda2<A, B, C> _f;
            private Behavior<B> b;

            public SnapshotEventSink(Event<A> ev, Lambda2<A, B, C> f, Behavior<B> b)
            {
                this.ev = ev;
                _f = f;
                this.b = b;
            }

            protected internal override Object[] sampleNow()
            {
                Object[] oi = ev.sampleNow();
                if (oi != null)
                {
                    Object[] oo = new Object[oi.Length];
                    for (int i = 0; i < oo.Length; i++)
                        oo[i] = _f.apply((A)oi[i], b.sample());
                    return oo;
                }
                else
                    return null;
            }
        }

        ///
        /// Merge two streams of events of the same type.
        ///
        /// In the case where two event occurrences are simultaneous (i.e. both
        /// within the same transaction), both will be delivered in the same
        /// transaction. If the event firings are ordered for some reason, then
        /// their ordering is retained. In many common cases the ordering will
        /// be undefined.
        ///
        public static Event<A> merge<A>(Event<A> ea, Event<A> eb)
        {
            MergeEventSink<A> out_ = new MergeEventSink<A>(ea, eb);
            TransactionHandler<A> h = new TransactionHandlerImpl<A>(out_.send);
            Listener l1 = ea.listen_(out_.node, h);
            Listener l2 = eb.listen_(out_.node, h);
            return out_.addCleanup(l1).addCleanup(l2);
        }

        private class MergeEventSink<A> : EventSink<A>
        {
            private Event<A> ea;
            private Event<A> eb;

            public MergeEventSink(Event<A> ea, Event<A> eb)
            {
                this.ea = ea;
                this.eb = eb;
            }

            protected internal override Object[] sampleNow()
            {
                Object[] oa = ea.sampleNow();
                Object[] ob = eb.sampleNow();
                if (oa != null && ob != null)
                {
                    Object[] oo = new Object[oa.Length + ob.Length];
                    int j = 0;
                    for (int i = 0; i < oa.Length; i++) oo[j++] = oa[i];
                    for (int i = 0; i < ob.Length; i++) oo[j++] = ob[i];
                    return oo;
                }
                else
                    if (oa != null)
                        return oa;
                    else
                        return ob;
            }
        }

        ///
        /// Push each event occurrence onto a new transaction.
        ///
        public Event<A> delay()
        {
            EventSink<A> out_ = new EventSink<A>();
            Listener l1 = listen_(out_.node, new TransactionHandlerImpl<A>((t, a) =>
                                                                               {
                                                                                   t.post(new RunnableImpl(() =>
                                                                                                               {
                                                                                                                   Transaction trans = new Transaction();
                                                                                                                   try
                                                                                                                   {
                                                                                                                       out_.send(trans, a);
                                                                                                                   }
                                                                                                                   finally
                                                                                                                   {
                                                                                                                       trans.close();
                                                                                                                   }
                                                                                                               }));
                                                                               }));


            return out_.addCleanup(l1);
        }

        /// <summary>
        /// Overload of coalese that accepts a Func<A,A,A> to support C# lambdas
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public Event<A> coalesce(Func<A, A, A> f)
        {
            return coalesce(new Lambda2Impl<A, A, A>(f));
        }

        ///
        /// If there's more than one firing in a single transaction, combine them into
        /// one using the specified combining function.
        ///
        /// If the event firings are ordered, then the first will appear at the left
        /// input of the combining function. In most common cases it's best not to
        /// make any assumptions about the ordering, and the combining function would
        /// ideally be commutative.
        ///
        public Event<A> coalesce(Lambda2<A, A, A> f)
        {
            return Transaction.apply(new Lambda1Impl<Transaction, Event<A>>(t => coalesce(t, f)));
        }

        Event<A> coalesce(Transaction trans1, Lambda2<A, A, A> f)
        {
            Event<A> ev = this;
            EventSink<A> out_ = new CoalesceEventSink<A>(ev, f);
            TransactionHandler<A> h = new CoalesceHandler<A>(f, out_);
            Listener l = listen(out_.node, trans1, h, false);
            return out_.addCleanup(l);
        }

        private class CoalesceEventSink<A> : EventSink<A>
        {
            private Event<A> ev;
            private Lambda2<A, A, A> f;

            public CoalesceEventSink(Event<A> ev, Lambda2<A, A, A> f)
            {
                this.ev = ev;
                this.f = f;
            }

            protected internal override Object[] sampleNow()
            {
                Object[] oi = ev.sampleNow();
                if (oi != null)
                {
                    A o = (A)oi[0];
                    for (int i = 1; i < oi.Length; i++)
                        o = f.apply(o, (A)oi[i]);
                    return new Object[] { o };
                }
                else
                    return null;
            }
        }

        ///
        /// Clean up the output by discarding any firing other than the last one. 
        ///
        internal Event<A> lastFiringOnly(Transaction trans)
        {
            return coalesce(trans, new Lambda2Impl<A, A, A>((a, b) => b));
        }

        ///
        /// Merge two streams of events of the same type, combining simultaneous
        /// event occurrences.
        ///
        /// In the case where multiple event occurrences are simultaneous (i.e. all
        /// within the same transaction), they are combined using the same logic as
        /// 'coalesce'.
        ///
        public static Event<A> mergeWith<A>(Lambda2<A, A, A> f, Event<A> ea, Event<A> eb)
        {
            return merge(ea, eb).coalesce(f);
        }

        /// <summary>
        /// Overload of filter that accepts a Func<A,Bool> to support C# lambda expressions
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public Event<A> filter(Func<A, bool> f)
        {
            return filter(new Lambda1Impl<A, bool>(f));
        }

        ///
        /// Only keep event occurrences for which the predicate returns true.
        ///
        public Event<A> filter(Lambda1<A, Boolean> f)
        {
            Event<A> ev = this;
            EventSink<A> out_ = new FilterEventSink<A>(ev, f);

            Listener l = listen_(out_.node,
                                 new TransactionHandlerImpl<A>((t, a) => { if (f.apply(a)) out_.send(t, a); }));
            return out_.addCleanup(l);
        }

        private class FilterEventSink<A> : EventSink<A>
        {
            private Event<A> ev;
            private Lambda1<A, Boolean> f;

            public FilterEventSink(Event<A> ev, Lambda1<A, Boolean> f)
            {
                this.ev = ev;
                this.f = f;
            }

            protected internal override Object[] sampleNow()
            {
                Object[] oi = ev.sampleNow();
                if (oi != null)
                {
                    Object[] oo = new Object[oi.Length];
                    int j = 0;
                    for (int i = 0; i < oi.Length; i++)
                        if (f.apply((A)oi[i]))
                            oo[j++] = oi[i];
                    if (j == 0)
                        oo = null;
                    else if (j < oo.Length)
                    {
                        Object[] oo2 = new Object[j];
                        for (int i = 0; i < j; i++)
                            oo2[i] = oo[i];
                        oo = oo2;
                    }
                    return oo;
                }
                else
                    return null;
            }
        }

        ///
        /// Filter out any event occurrences whose value is a Java null pointer.
        ///
        public Event<A> filterNotNull()
        {
            return filter(new Lambda1Impl<A, Boolean>(a => a != null));
        }

        ///
        /// Let event occurrences through only when the behavior's value is True.
        /// Note that the behavior's value is as it was at the start of the transaction,
        /// that is, no state changes from the current transaction are taken into account.
        ///
        public Event<A> gate(Behavior<Boolean> bPred)
        {
            var f = new Lambda2Impl<A, bool, Maybe<A>>((a, pred) => pred ? new Maybe<A>(a) : null);
            return snapshot(bPred, f).filterNotNull().map(a => a.Value());
        }

        ///
        /// Transform an event with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        ///
        public Event<B> collect<B, S>(S initState, Lambda2<A, S, Tuple2<B, S>> f)
        {
            Event<A> ea = this;
            EventLoop<S> es = new EventLoop<S>();
            Behavior<S> s = es.hold(initState);
            Event<Tuple2<B, S>> ebs = ea.snapshot(s, f);
            Event<B> eb = ebs.map(new Lambda1Impl<Tuple2<B, S>, B>(bs => bs.a));
            Event<S> es_out = ebs.map(new Lambda1Impl<Tuple2<B, S>, S>(bs => bs.b));
            es.loop(es_out);
            return eb;
        }


        public Behavior<S> accum<S>(S initState, Func<A, S, S> f)
        {
            return accum(initState, new Lambda2Impl<A, S, S>(f));
        }

        ///
        /// Accumulate on input event, outputting the new state each time.
        ///
        public Behavior<S> accum<S>(S initState, Lambda2<A, S, S> f)
        {
            Event<A> ea = this;
            EventLoop<S> es = new EventLoop<S>();
            Behavior<S> s = es.hold(initState);
            Event<S> es_out = ea.snapshot(s, f);
            es.loop(es_out);
            return es_out.hold(initState);
        }

        ///
        /// Throw away all event occurrences except for the first one.
        ///
        public Event<A> once()
        {
            // This is a bit long-winded but it's efficient because it deregisters
            // the listener.
            Event<A> ev = this;
            Listener[] la = new Listener[1];
            EventSink<A> out_ = new OnceEventSink<A>(ev, la);
            la[0] = ev.listen_(out_.node, new TransactionHandlerImpl<A>((t, a) =>
            {
                out_.send(t, a);
                if (la[0] != null)
                {
                    la[0].unlisten();
                    la[0] = null;
                }
            }));
            return out_.addCleanup(la[0]);
        }

        private class OnceEventSink<A> : EventSink<A>
        {
            private Event<A> ev;
            private Listener[] la;

            public OnceEventSink(Event<A> ev, Listener[] la)
            {
                this.ev = ev;
                this.la = la;
            }

            protected internal override Object[] sampleNow()
            {
                Object[] oi = ev.sampleNow();
                Object[] oo = oi;
                if (oo != null)
                {
                    if (oo.Length > 1)
                        oo = new Object[] { oi[0] };
                    if (la[0] != null)
                    {
                        la[0].unlisten();
                        la[0] = null;
                    }
                }
                return oo;
            }
        }

        internal Event<A> addCleanup(Listener cleanup)
        {
            finalizers.Add(cleanup);
            return this;
        }

        ~Event()
        {
            foreach (Listener l in finalizers)
                l.unlisten();
        }

        private class CoalesceHandler<A> : TransactionHandler<A>
        {
            public CoalesceHandler(Lambda2<A, A, A> f, EventSink<A> out_)
            {
                this.f = f;
                this.out_ = out_;
            }

            private Lambda2<A, A, A> f;
            private EventSink<A> out_;

            private Maybe<A> accum = Maybe<A>.Null;

            public void run(Transaction trans1, A a)
            {
                if (accum.HasValue)
                    accum = new Maybe<A>(f.apply(accum.Value(), a));
                else
                {
                    CoalesceHandler<A> thiz = this;
                    trans1.prioritized(out_.node, new HandlerImpl<Transaction>((t) =>
                    {
                        out_.send(t, thiz.accum.Value());
                        thiz.accum = Maybe<A>.Null;
                    }));
                    accum = new Maybe<A>(a);
                }
            }
        }

    }
}