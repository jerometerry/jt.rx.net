namespace Sodium.MemoryTests
{
    using NUnit.Framework;

    [TestFixture]
    public class MemoryTest4
    {
        [TestCase(1000000000)]
        public void Test(int iterations)
        {
            var evt = new Event<int>();
            IEvent<int> nestedEvent = new Event<int>();
            var behaviorOfEvents = evt.Map(x => nestedEvent).Hold(nestedEvent);
            var observable = Behavior<int>.SwitchE(behaviorOfEvents);
            var listen = observable.Subscribe(tt => { });
            var i = 0;
            while (i < iterations)
            {
                evt.Fire(i);
                i++;
            }

            listen.Dispose();
            observable.Dispose();
        }
    }
}