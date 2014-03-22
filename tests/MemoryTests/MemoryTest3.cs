namespace Potassium.MemoryTests
{
    using Potassium.Core;
    using Potassium.Extensions;
    using NUnit.Framework;

    [TestFixture]
    public class MemoryTest3
    {
        [TestCase(1000000000)]
        public void Test(int iterations)
        {
            var evt = new EventPublisher<int>();
            var et = new Event<int>();
            var behavior = et.Hold(0);
            var eventOfBehaviors = evt.Map(x => behavior).Hold(behavior);
            var observable = eventOfBehaviors.Switch();
            var values = observable.Values();
            var l = values.Subscribe(tt => { });
            var i = 0;
            while (i < iterations)
            {
                evt.Publish(i);
                i++;
            }

            l.Dispose();
            values.Dispose();
        }
    }
}