// -----------------------------------------------------------------------
// <copyright file="FixedQueue.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.WinForm
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>A queue with a fixed capacity, dequeuing when capacity is reached.</summary>
    /// <typeparam name="T">Type of queue.</typeparam>
    public class FixedQueue<T> : Queue<T>
    {
        private const int DefaultCapacity = 50;

        /// <summary>Initialises a new instance of the <see cref="Serilog.Sinks.WinForm.FixedQueue{T}" /> class.</summary>
        /// <param name="capacity">Size of queue.</param>
        public FixedQueue(int capacity)
            : base(capacity) =>
            this.Capacity = capacity;

        /// <summary>
        /// Initialises a new instance of the <see cref="Serilog.Sinks.WinForm.FixedQueue{T}" /> class. With a default.
        /// capacity of 50.
        /// </summary>
        public FixedQueue()
            : this(DefaultCapacity)
        {
            // empty
        }

        /// <summary>Initialises a new instance of the <see cref="Serilog.Sinks.WinForm.FixedQueue{T}" /> class.</summary>
        /// <param name="collection">Default collection and capacity size.</param>
        public FixedQueue(IEnumerable<T> collection)
            : base(collection) =>
            this.Capacity = collection.Count();

        /// <summary>Gets or sets capacity of queue.</summary>
        public int Capacity { get; set; }

        /// <summary>Adds <paramref name="item" /> to the tail of the queue. Removin from head as required.</summary>
        /// <param name="item">Item to be added.</param>
        public new void Enqueue(T item)
        {
            if (this.Count == this.Capacity)
            {
                // remove an item
                if (!this.TryDequeue(out _))
                {
                    throw new InvalidOperationException("Unable to dequeue from queue.");
                }

                base.Enqueue(item);
                return;
            }

            base.Enqueue(item);
        }
    }
}