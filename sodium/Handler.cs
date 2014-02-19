﻿namespace sodium
{
    using System;

    sealed class Handler<T> : IHandler<T>
    {
        readonly Action<T> _function;

        public Handler(Action<T> function)
        {
            _function = function;
        }

        public void Run(T p)
        {
            _function(p);
        }

        public void Dispose()
        {
        }
    }
}
