﻿namespace Potassium.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Base class for all disposable objects in the Sodium library.
    /// </summary>
    public class Disposable : IDisposable
    {
        private List<IDisposable> finalizers;

        /// <summary>
        /// Constructs a new SodiumObject
        /// </summary>
        protected Disposable()
        {
        }

        /// <summary>
        /// Gets whether the current SodiumObject is disposed
        /// </summary>
        public bool Disposed { get; private set; }
        
        /// <summary>
        /// Gets whether the current SodiumObject is being disposed.
        /// </summary>
        public bool Disposing { get; private set; }

        /// <summary>
        /// Registers the given SodiumObject to be disposed when the current
        /// SodiumObject is disposed.
        /// </summary>
        /// <param name="o">The SodiumObject to register for disposal</param>
        public void Register(IDisposable o)
        {
            if (finalizers == null)
            {
                finalizers = new List<IDisposable>();
            }

            finalizers.Add(o);
        }

        /// <summary>
        /// Disposes the current SodiumObject
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up all resources used by the current SodiumObject
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.Disposed || this.Disposing)
            {
                return;
            }

            this.Disposing = true;

            if (finalizers != null && finalizers.Count > 0)
            {
                foreach (var item in finalizers)
                {
                    item.Dispose();
                }

                finalizers.Clear();
                finalizers = null;
            }

            this.Disposed = true;
            this.Disposing = false;
        }
    }
}
