using System;

namespace ServerShared.Sources
{
    public class GrowingBuffer<T>
    {
        private T[] _buffer;

        public T[] Array {
            get { return _buffer; }
        }

        public T[] GetBuffer(int size)
        {
            EnsureSize(size);
            return _buffer;
        }

        public GrowingBuffer(int initialSize)
        {
            _buffer = new T[initialSize];
        }

        public ArraySegment<T> GetSegment(int offset, int size)
        {
            return new ArraySegment<T>(_buffer, offset, size);
        }

        public void EnsureSize(int size, bool copy = false)
        {
            if(_buffer.Length >= size)
                return;

            var t = new T[size];
            if (copy)
            { 
                System.Array.Copy(_buffer, t, _buffer.Length);
            }
            _buffer = t;
        }
    }
}
