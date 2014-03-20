﻿namespace Sodium
{
    using System;

    /// <summary>
    /// QueryBehavior is a continuous Behavior who's value is computed using a query method.
    /// </summary>
    /// <typeparam name="T">The type of the return value of the query function</typeparam>
    public class QueryBehavior<T> : ContinuousBehavior<T>
    {
        private Func<T> query;

        public QueryBehavior(Func<T> query)
        {
            this.query = query;
        }

        public override T Value
        {
            get
            {
                return query();
            }
        }
    }
}
