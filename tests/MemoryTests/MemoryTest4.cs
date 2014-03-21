namespace Sodium.MemoryTests
{
    using Sodium.Core;
    using Sodium.Discrete;
    using NUnit.Framework;

    [TestFixture]
    public class MemoryTest4
    {
        [TestCase(1000000000)]
        public void Test(int iterations)
        {
            var evt = new EventPublisher<int>();
            Event<int> nestedEvent = new Event<int>();
            var behaviorOfEvents = evt.Map(x => nestedEvent).Hold(nestedEvent);
            var observable = behaviorOfEvents.Switch();
            var listen = observable.Subscribe(tt => { });
            var i = 0;
            while (i < iterations)
            {
                evt.Publish(i);
                i++;
            }

            listen.Dispose();
            observable.Dispose();
        }
    }
}