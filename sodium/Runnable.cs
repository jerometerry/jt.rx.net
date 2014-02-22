using System;

namespace Sodium
{
    internal sealed class Runnable : IRunnable
    {
        private readonly Action _action;

        public Runnable(Action action)
        {
            _action = action;
        }

        public void Run()
        {
            _action();
        }
    }
}