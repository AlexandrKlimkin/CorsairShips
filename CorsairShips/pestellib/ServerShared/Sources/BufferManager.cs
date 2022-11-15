using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared
{
    public class BufferManager<T>
    {
        private readonly int _maxSize;
        private readonly Dictionary<int, Queue<T[]>> _buffers = new Dictionary<int, Queue<T[]>>();
        private readonly object _lock = new object();

        public BufferManager()
            :this(int.MaxValue)
        {
        }

        public BufferManager(int maxSize)
        {
            _maxSize = maxSize;
        }

        public ObjectScopeGuard<T[]> AllocBufferGuard(int size)
        {
            return new ObjectScopeGuard<T[]>(() => AllocBuffer(size), (d) => FreeBuffer(d));
        }

        public T[] AllocBuffer(int size)
        {
            if (size > _maxSize)
                throw new InvalidOperationException("Allocation size " + size + " exceeds the limit of " + _maxSize);
            lock (_lock)
            {
                var bits = (int)Math.Log(size, 2) + 1;
                var sz = (1 << bits) - 1;
                Queue<T[]> bufs;
                if (!_buffers.TryGetValue(sz, out bufs))
                    return new T[sz];

                if (bufs.Count > 0)
                    return bufs.Dequeue();

                return new T[sz];
            }
        }

        public void AllocExchange(ref T[] buffer, int size)
        {
            var oldBuffer = buffer;
            buffer = AllocBuffer(size);
            if (oldBuffer != null)
                FreeBuffer(oldBuffer);
        }

        public T[] Resize(T[] old, int newSize)
        {
            if (newSize < old.Length)
                return old;

            var result = AllocBuffer(newSize);
            old.CopyTo(result, 0);
            FreeBuffer(old);

            return result;
        }

        public void FreeBuffer(T[] data)
        {
            if(data.Length > _maxSize)
                return;
            lock (_lock)
            {
                Array.Clear(data, 0, data.Length);

                Queue<T[]> bufs;
                if (_buffers.TryGetValue(data.Length, out bufs))
                {
                    bufs.Enqueue(data);
                    return;
                }

                bufs = new Queue<T[]>();
                bufs.Enqueue(data);
                _buffers[data.Length] = bufs;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _buffers.Clear();
            }
        }

        public override string ToString()
        {
            var total = _buffers.Select(x => x.Key * x.Value.Count).Sum();
            return string.Format("BufferManager reserved({0}):\n{1}", total,
                string.Join("\n", _buffers.Select(x => string.Format("{0}: {1}", x.Key, x.Value.Count)).ToArray()));
        }

    }
}
