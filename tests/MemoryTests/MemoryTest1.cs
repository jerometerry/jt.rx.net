namespace Sodium.MemoryTests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;

    [TestFixture]
    public class MemoryTest1
    {
        [TestCase(100)]
        [TestCase(1000000000)]
        public void Test(int iterations)
        {
            var finalizers = new List<SodiumItem>();

            var behaviorMapFinalizers = new List<SodiumItem>();

            var evt = new Event<int?>(false) { Description = "Root event that will be fired on" };
            finalizers.Add(evt);

            var behavior = evt.Hold(0, false);
            behavior.Description = "Behavior that gets updates from the root event";
            finalizers.Add(behavior);

            // TODO - etens isn't being used. Seems like it should be
            var etens = evt.Map(x => x / 10, false);
            etens.Description = "Root event mapped to values divided by ten";
            finalizers.Add(etens);

            var snapshotEvent = evt.Snapshot(behavior, (neu, old) => neu.Equals(old) ? null : neu, false);
            snapshotEvent.Description = "Event<int?> that fires the snapshot function when the root event is fired. " + 
                "Neu is the value fired on the root event, old is the value of the behavior at the time of the firing";
            finalizers.Add(snapshotEvent);

            var changeTens = snapshotEvent.FilterNotNull(false);
            changeTens.Description = "Event<int?> that filters out null values from the snapshot event, giving only the events when the tens changes";
            finalizers.Add(changeTens);

            var eventOfBehaviors = changeTens.Map(
                tens =>
                    {
                        // TODO - uncommenting out this line causes an ObjectDisposedException
                        // TODO - The result of behavior.Map should be able to be AutoDisposed
                        //AutoDisposeFinalizers(behaviorMapFinalizers);
                        var tmp = behavior.Map(tt => new Tuple<int?, int?>(tens, tt), true);
                        tmp.Description = "Behavior<Tuple<int?,int?>>";
                        behaviorMapFinalizers.Add(tmp);
                        return tmp;
                    },
                false);
            eventOfBehaviors.Description = "Event<Behaviors<Tuple<int?,int?>>, fired when the tens changes";
            finalizers.Add(eventOfBehaviors);

            var behaviorMap = behavior.Map(tt => new Tuple<int?, int?>(0, tt), false);
            finalizers.Add(behaviorMap);

            var tensTupleWrappedBehavior = eventOfBehaviors.Hold(behaviorMap, false);
            tensTupleWrappedBehavior.Description = "Behavior<Behaviors<Tuple<int?,int?>>";
            finalizers.Add(tensTupleWrappedBehavior);

            var tensTupleBehavior = Behavior<Tuple<int?, int?>>.SwitchB(tensTupleWrappedBehavior, false);
            tensTupleBehavior.Description = "Behavior<Tuple<int?, int?>>";
            finalizers.Add(tensTupleBehavior);

            var tensTupleEvent = tensTupleBehavior.Value(false);
            tensTupleEvent.Description = "Event<Tuple<int?,int?>>";
            finalizers.Add(tensTupleEvent);

            var listener = tensTupleEvent.Listen(tu => { });
            listener.Description = "Listener for tens changes";

            var i = 0;

            while (i < iterations)
            {
                evt.Fire(i);
                i++;
            }

            DisposeFinalizers(finalizers);
            DisposeFinalizers(behaviorMapFinalizers);

            listener.Dispose();
            
            Assert.AreEqual(0, Metrics.LiveItemCount);
        }

        private static void DisposeFinalizers(List<SodiumItem> items)
        {
            foreach (var item in items)
            {
                item.Dispose();
            }
            items.Clear();
        }

        private static void AutoDisposeFinalizers(List<SodiumItem> items)
        {
            foreach (var item in items)
            {
                item.AutoDispose();
            }
            items.Clear();
        }
    }
}