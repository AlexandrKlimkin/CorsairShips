using System;
using System.Collections.Generic;
using System.Text;

namespace ServerShared
{
    public class LimitedQueue<T> : Queue<T>
    {
        private readonly int _maxCapacity;

        public LimitedQueue(int maxCapacity)
        {
            if(maxCapacity < 1)
                throw new Exception("Capacity < 1. Use regular Queue.");
            _maxCapacity = maxCapacity;
        }

        public new void Enqueue(T item)
        {
            if (Count == _maxCapacity) Dequeue();
            base.Enqueue(item);
        }
    }
}
