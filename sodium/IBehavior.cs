﻿namespace Sodium
{
    using System;

    public interface IBehavior<T> : IObservableBehavior<T>, IValue<T>
    {
        IBehavior<TB> Apply<TB>(IBehavior<Func<T, TB>> bf);
        
        IBehavior<TB> Collect<TB, TS>(TS initState, Func<T, TS, Tuple<TB, TS>> snapshot);
        
        IBehavior<TD> Lift<TB, TC, TD>(Func<T, TB, TC, TD> lift, IBehavior<TB> b, IBehavior<TC> c);
        
        IBehavior<TC> Lift<TB, TC>(Func<T, TB, TC> lift, IBehavior<TB> behavior);
        
        IBehavior<TB> Map<TB>(Func<T, TB> map);
    }   
}
