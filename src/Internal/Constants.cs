﻿namespace Potassium.Internal
{
    using System;

    internal static class Constants
    {
        /// <summary>
        /// Fine-grained lock that protects listeners and nodes. 
        /// </summary>
        internal static readonly object SubscriptionLock = new object();

        /// <summary>
        /// Coarse-grained lock that's held during the whole transaction. 
        /// </summary>
        internal static readonly object TransactionLock = new object();

        /// <summary>
        /// 
        /// </summary>
        internal static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(1);
    }
}
