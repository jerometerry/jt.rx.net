namespace Sodium.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using NUnit.Framework;

    [TestFixture]
    public class BehaviorTester : SodiumTestCase
    {
        [Test]
        public void TestHold()
        {
            var evt = new Event<int>();
            var behavior = evt.Hold(0);
            var results = new List<int>();
            var listener = behavior.Updates().Listen(results.Add);
            evt.Fire(2);
            evt.Fire(9);
            listener.Stop();
            AssertArraysEqual(Arrays<int>.AsList(2, 9), results);
        }

        [Test]
        public void TestSnapshot()
        {
            var behavior = Behavior<int>.Sink(0);
            var evt = new Event<long>();
            var results = new List<string>();
            Func<long, int, string> snapshotFunction = (x, y) => string.Format("{0} {1}", x, y);
            var listener = evt.Snapshot(behavior, snapshotFunction).Listen(results.Add);

            evt.Fire(100L);
            behavior.Fire(2);
            evt.Fire(200L);
            behavior.Fire(9);
            behavior.Fire(1);
            evt.Fire(300L);
            listener.Stop();
            AssertArraysEqual(Arrays<string>.AsList("100 0", "200 2", "300 1"), results);
        }

        [Test]
        public void TestValues()
        {
            var behavior = Behavior<int>.Sink(9);
            var results = new List<int>();
            var listener = behavior.Value().Listen(results.Add);
            behavior.Fire(2);
            behavior.Fire(7);
            listener.Stop();
            AssertArraysEqual(Arrays<int>.AsList(9, 2, 7), results);
        }

        [Test]
        public void TestConstantBehavior()
        {
            var behavior = new Behavior<int>(12);
            var results = new List<int>();
            var listener = behavior.Value().Listen(results.Add);
            listener.Stop();
            AssertArraysEqual(Arrays<int>.AsList(12), results);
        }

        [Test]
        public void TestValuesThenMap()
        {
            var behavior = Behavior<int>.Sink(9);
            var results = new List<int>();
            var l = behavior.Value().Map(x => x + 100).Listen(results.Add);
            behavior.Fire(2);
            behavior.Fire(7);
            l.Stop();
            AssertArraysEqual(Arrays<int>.AsList(109, 102, 107), results);
        }

        [Test]
        public void TestValuesTwiceThenMap()
        {
            var behavior = Behavior<int>.Sink(9);
            var results = new List<int>();
            var listener = DoubleUp(behavior.Value()).Map(x => x + 100).Listen(results.Add);
            behavior.Fire(2);
            behavior.Fire(7);
            listener.Stop();
            AssertArraysEqual(Arrays<int>.AsList(109, 109, 102, 102, 107, 107), results);
        }

        [Test]
        public void TestValuesThenCoalesce()
        {
            var behavior = Behavior<int>.Sink(9);
            var results = new List<int>();
            var listener = behavior.Value().Coalesce((fst, snd) => snd).Listen(results.Add);
            behavior.Fire(2);
            behavior.Fire(7);
            listener.Stop();
            AssertArraysEqual(Arrays<int>.AsList(9, 2, 7), results);
        }

        [Test]
        public void TestValuesTwiceThenCoalesce() 
        {
            var behavior = Behavior<int>.Sink(9);
            var results = new List<int>();
            var listener = DoubleUp(behavior.Value()).Coalesce((fst, snd) => fst + snd).Listen(results.Add);
            behavior.Fire(2);
            behavior.Fire(7);
            listener.Stop();
            AssertArraysEqual(Arrays<int>.AsList(18, 4, 14), results);
        }
        
        [Test]
        public void TestValuesThenSnapshot() 
        {
            var behaviorInt32 = Behavior<int>.Sink(9);
            var behaviorChar = Behavior<char>.Sink('a');
            var results = new List<char>();
            var listener = behaviorInt32.Value().Snapshot(behaviorChar).Listen(results.Add);
            behaviorChar.Fire('b');
            behaviorInt32.Fire(2);
            behaviorChar.Fire('c');
            behaviorInt32.Fire(7);
            listener.Stop();
            AssertArraysEqual(Arrays<char>.AsList('a', 'b', 'c'), results);
        }
        
        [Test]
        public void TestValuesTwiceThenSnapshot() 
        {
            var behaviorInt32 = Behavior<int>.Sink(9);
            var behaviorChar = Behavior<char>.Sink('a');
            var results = new List<char>();
            var listener = DoubleUp(behaviorInt32.Value()).Snapshot(behaviorChar).Listen(results.Add);
            behaviorChar.Fire('b');
            behaviorInt32.Fire(2);
            behaviorChar.Fire('c');
            behaviorInt32.Fire(7);
            listener.Stop();
            AssertArraysEqual(Arrays<char>.AsList('a', 'a', 'b', 'b', 'c', 'c'), results);
        }
        
        [Test]
        public void TestValuesThenMerge() 
        {
            var behavior1 = Behavior<int>.Sink(9);
            var behavior2 = Behavior<int>.Sink(2);
            var results = new List<int>();
            var listener = Event<int>.MergeWith((x, y) => x + y, behavior1.Value(), behavior2.Value()).Listen(results.Add);
            behavior1.Fire(1);
            behavior2.Fire(4);
            listener.Stop();
            AssertArraysEqual(Arrays<int>.AsList(11, 1, 4), results);
        }
        
        [Test]
        public void TestValuesThenFilter() 
        {
            var behavior = Behavior<int>.Sink(9);
            var results = new List<int>();
            var listener = behavior.Value().Filter(a => true).Listen(results.Add);
            behavior.Fire(2);
            behavior.Fire(7);
            listener.Stop();
            AssertArraysEqual(Arrays<int>.AsList(9, 2, 7), results);
        }
        
        [Test]
        public void TestValuesTwiceThenFilter() 
        {
            var behavior = Behavior<int>.Sink(9);
            var results = new List<int>();
            var listener = DoubleUp(behavior.Value()).Filter(a => true).Listen(results.Add);
            behavior.Fire(2);
            behavior.Fire(7);
            listener.Stop();
            AssertArraysEqual(Arrays<int>.AsList(9, 9, 2, 2, 7, 7), results);
        }
        
        [Test]
        public void TestValuesThenOnce() 
        {
            var behavior = Behavior<int>.Sink(9);
            var results = new List<int>();
            var listener = behavior.Value().Once().Listen(results.Add);
            behavior.Fire(2);
            behavior.Fire(7);
            listener.Stop();
            AssertArraysEqual(Arrays<int>.AsList(9), results);
        }
        
        [Test]
        public void TestValuesTwiceThenOnce() 
        {
            var behavior = Behavior<int>.Sink(9);
            var results = new List<int>();
            var listener = DoubleUp(behavior.Value()).Once().Listen(results.Add);
            behavior.Fire(2);
            behavior.Fire(7);
            listener.Stop();
            AssertArraysEqual(Arrays<int>.AsList(9), results);
        }

        [Test]
        public void TestValuesLateListen() 
        {
            var behavior = Behavior<int>.Sink(9);
            var results = new List<int>();
            var value = behavior.Value();
            behavior.Fire(8);
            var listener = value.Listen(results.Add);
            behavior.Fire(2);
            listener.Stop();
            AssertArraysEqual(Arrays<int>.AsList(8, 2), results);
        }
	
        [Test]
        public void TestMapB() 
        {
            var behavior = Behavior<int>.Sink(6);
            var results = new List<string>();
            var listener = behavior.Map(x => x.ToString(CultureInfo.InvariantCulture)).Value().Listen(results.Add);
            behavior.Fire(8);
            listener.Stop();
            AssertArraysEqual(Arrays<string>.AsList("6", "8"), results);
        }

        [Test]
        public void TestMapB2()
        {
            var behavior = Behavior<int>.Sink(1);
            var behavior1 = behavior.Map(x => x * 3);
            var results = new List<int>();
            var listener = behavior1.Value().Listen(results.Add);
            listener.Stop();
            AssertArraysEqual(Arrays<int>.AsList(3), results);
        }

        [Test]
        public void TestMapB3()
        {
            var behavior = Behavior<int>.Sink(1);
            var behavior1 = behavior.Map(x => x * 3);
            var results = new List<int>();
            var listener = behavior1.Value().Listen(results.Add);
            behavior.Fire(2);
            listener.Stop();
            AssertArraysEqual(Arrays<int>.AsList(3, 6), results);
        }
	
        [Test]
        public void TestMapBLateListen() 
        {
            var behavior = Behavior<int>.Sink(6);
            var results = new List<string>();
            var map = behavior.Map(x => x.ToString(CultureInfo.InvariantCulture));
            behavior.Fire(2);
            var listener = map.Value().Listen(results.Add);
            behavior.Fire(8);
            listener.Stop();
            AssertArraysEqual(Arrays<string>.AsList("2", "8"), results);
        }

        [Test]
        public void TestApply()
        {
            var bf = Behavior<Func<long, string>>.Sink(b => "1 " + b);
            var ba = Behavior<long>.Sink(5L);
            var results = new List<string>();
            var listener = Behavior<long>.Apply(bf, ba).Value().Listen(results.Add);
            bf.Fire(b => "12 " + b);
            ba.Fire(6L);
            listener.Stop();
            AssertArraysEqual(Arrays<string>.AsList("1 5", "12 5", "12 6"), results);
        }

        [Test]
        public void TestLift() 
        {
            var behavior1 = Behavior<int>.Sink(1);
            var behavior2 = Behavior<long>.Sink(5L);
            var results = new List<string>();
            var combinedBehavior = Behavior<int>.Lift((x, y) => x + " " + y, behavior1, behavior2);
            var listener = combinedBehavior.Value().Listen(results.Add);
            behavior1.Fire(12);
            behavior2.Fire(6L);
            listener.Stop();
            AssertArraysEqual(Arrays<string>.AsList("1 5", "12 5", "12 6"), results);
        }

        [Test]
        public void TestMapAndLift() 
        {
            var behavior = Behavior<int>.Sink(1);
            var mappedBehavior1 = behavior.Map(x => x * 3);
            var mappedBehavior2 = behavior.Map(x => x * 5);
            var results = new List<string>();
            var combinedBehavior = Behavior<int>.Lift((x, y) => x + " " + y, mappedBehavior1, mappedBehavior2);
            var listener = combinedBehavior.Value().Listen(results.Add);
            behavior.Fire(2);
            listener.Stop();
            AssertArraysEqual(Arrays<string>.AsList("3 5", "6 10"), results);
        }

        [Test]
        public void TestHoldIsDelayed() 
        {
            var e = new Event<int>();
            var h = e.Hold(0);
            var pair = e.Snapshot(h, (a, b) => a + " " + b);
            var o = new List<string>();
            var l = pair.Listen(o.Add);
            e.Fire(2);
            e.Fire(3);
            l.Stop();
            AssertArraysEqual(Arrays<string>.AsList("2 0", "3 2"), o);
        }

        [Test]
        public void TestSwitchB()
        {
            var sink = new Event<Sb>();

            // Split each field o of SB so we can update multiple behaviours in a
            // single transaction.
            var behaviorA = sink.Map(s => s.C1).FilterNotNull().Hold('A');
            var behaviorB = sink.Map(s => s.C2).FilterNotNull().Hold('a');
            var bsw = sink.Map(s => s.Behavior).FilterNotNull().Hold(behaviorA);
            var behavior = Behavior<char?>.SwitchB(bsw);
            var results = new List<char>();
            var listener = behavior.Value().Listen(c =>
            {
                Assert.IsNotNull(c, "c != null");
                results.Add(c.Value);
            });
            sink.Fire(new Sb('B','b',null));
            sink.Fire(new Sb('C','c',behaviorB));
            sink.Fire(new Sb('D','d',null));
            sink.Fire(new Sb('E','e',behaviorA));
            sink.Fire(new Sb('F','f',null));
            sink.Fire(new Sb(null,null,behaviorB));
            sink.Fire(new Sb(null,null,behaviorA));
            sink.Fire(new Sb('G','g',behaviorB));
            sink.Fire(new Sb('H','h',behaviorA));
            sink.Fire(new Sb('I','i',behaviorA));
            listener.Stop();
            AssertArraysEqual(Arrays<char>.AsList('A','B','c','d','E','F','f','F','g','H','I'), results);
        }

        [Test]
        public void TestSwitchB1()
        {
            var sink = new Event<Sb>();

            // Split each field o of SB so we can update multiple behaviours in a
            // single transaction.
            var behaviorA = sink.Map(s => s.C1).FilterNotNull().Hold('A');
            var behaviorB = sink.Map(s => s.C2).FilterNotNull().Hold('a');
            var bsw = sink.Map(s => s.Behavior).FilterNotNull().Hold(behaviorA);
            var behavior = Behavior<char?>.SwitchB(bsw);
            var results = new List<char>();
            var listener = behavior.Value().Listen(c =>
            {
                Assert.IsNotNull(c, "c != null");
                results.Add(c.Value);
            });
            sink.Fire(new Sb('B', 'b', null));
            listener.Stop();
            AssertArraysEqual(Arrays<char>.AsList('A', 'B'), results);
        }

        [Test]
        public void TestSwitchE()
        {
            var ese = new Event<Se>();
            var ea = ese.Map(s => s.C1).FilterNotNull();
            var eb = ese.Map(s => s.C2).FilterNotNull();
            var bsw = ese.Map(s => s.Event).FilterNotNull().Hold(ea);
            var o = new List<char>();
            var eo = Behavior<char>.SwitchE(bsw);
            var l = eo.Listen(o.Add);
            ese.Fire(new Se('A','a',null));
            ese.Fire(new Se('B','b',null));
            ese.Fire(new Se('C','c',eb));
            ese.Fire(new Se('D','d',null));
            ese.Fire(new Se('E','e',ea));
            ese.Fire(new Se('F','f',null));
            ese.Fire(new Se('G','g',eb));
            ese.Fire(new Se('H','h',ea));
            ese.Fire(new Se('I','i',ea));
            l.Stop();
            AssertArraysEqual(Arrays<char>.AsList('A','B','C','d','e','F','G','h','I'), o);
        }

        [Test]
        public void TestLoopBehavior()
        {
            var ea = new Event<int>();
            var sum = new Behavior<int>();
            var sumOut = ea.Snapshot(sum, (x, y) => x+y).Hold(0);
            sum.Loop(sumOut);
            var o = new List<int>();
            var l = sumOut.Value().Listen(o.Add);
            ea.Fire(2);
            ea.Fire(3);
            ea.Fire(1);
            l.Stop();
            AssertArraysEqual(Arrays<int>.AsList(0,2,5,6), o);
            Assert.AreEqual(6, sum.Sample());
        }

        [Test]
        public void TestCollect()
        {
            var ea = new Event<int>();
            var o = new List<int>();
            var sum = ea.Hold(100).Collect(0, (a, s) => new Tuple<int, int>(a + s, a + s));
            var l = sum.Value().Listen(o.Add);
            ea.Fire(5);
            ea.Fire(7);
            ea.Fire(1);
            ea.Fire(2);
            ea.Fire(3);
            l.Stop();
            AssertArraysEqual(Arrays<int>.AsList(100, 105, 112, 113, 115, 118), o);
        }

        [Test]
        public void TestAccum()
        {
            var ea = new Event<int>();
            var o = new List<int>();
            var sum = ea.Accum(100, (a, s) => a + s);
            var l = sum.Value().Listen(o.Add);
            ea.Fire(5);
            ea.Fire(7);
            ea.Fire(1);
            ea.Fire(2);
            ea.Fire(3);
            l.Stop();
            AssertArraysEqual(Arrays<int>.AsList(100,105,112,113,115,118), o);
        }
        
        /// <summary>
        /// This is used for tests where Value() produces a single initial value on listen,
        /// and then we double that up by causing that single initial event to be repeated.
        /// This needs testing separately, because the code must be done carefully to achieve
        /// this.
        /// </summary>
        private static Event<int> DoubleUp(Event<int> ev)
        {
            return Event<int>.Merge(ev, ev);
        }

        private class Se
        {
            public readonly char C1;
            public readonly char C2;
            public readonly Event<char> Event;

            public Se(char c1, char c2, Event<char> evt)
            {
                C1 = c1;
                C2 = c2;
                Event = evt;
            }
        }

        private class Sb
        {
            public readonly char? C1;
            public readonly char? C2;
            public readonly Behavior<char?> Behavior;

            public Sb(char? c1, char? c2, Behavior<char?> behavior)
            {
                C1 = c1;
                C2 = c2;
                Behavior = behavior;
            }
        }
    }
}