namespace Sodium.MemoryTests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;

    [TestFixture]
    public class MemoryTest1
    {
        [TestCase(1000000000)]
        public void Test(int iterations)
        {
            var finalizers = new List<IDisposable>();

            var behaviorMapFinalizers = new List<IDisposable>();

            var evt = new EventPublisher<int?>();
            finalizers.Add(evt);

            var behavior = evt.Hold(0);
            finalizers.Add(behavior);

            // TODO - etens isn't being used. Seems like it should be
            var etens = evt / 10;
            finalizers.Add(etens);

            var snapshotEvent = evt.Snapshot(behavior, (neu, old) => neu.Equals(old) ? null : neu);
            finalizers.Add(snapshotEvent);

            var changeTens = snapshotEvent.FilterNotNull();
            finalizers.Add(changeTens);

            var eventOfBehaviors = changeTens.Map(tens =>
            {
                DisposeFinalizers(behaviorMapFinalizers);
                var mapped = behavior.Map(tt => new Tuple<int?, int?>(tens, tt));
                behaviorMapFinalizers.Add(mapped);
                return mapped;
            });
            finalizers.Add(eventOfBehaviors);

            var behaviorMap = behavior.Map(tt => new Tuple<int?, int?>(0, tt));
            finalizers.Add(behaviorMap);

            var tensTupleWrappedBehavior = eventOfBehaviors.Hold(behaviorMap);
            finalizers.Add(tensTupleWrappedBehavior);

            var tensTupleBehavior = tensTupleWrappedBehavior.Switch();
            finalizers.Add(tensTupleBehavior);

            var values = tensTupleBehavior.Values();
            var listener = values.Subscribe(tu => { });
            var i = 0;

            while (i < iterations)
            {
                evt.Publish(i);
                i++;
            }

            listener.Dispose();
            values.Dispose();

            DisposeFinalizers(finalizers);
            DisposeFinalizers(behaviorMapFinalizers);
        }

        private static void DisposeFinalizers(List<IDisposable> items)
        {
            foreach (var item in items)
            {
                item.Dispose();
            }

            items.Clear();
        }
    }
}