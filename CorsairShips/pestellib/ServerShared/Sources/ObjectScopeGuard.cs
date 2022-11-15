using System;

namespace ServerShared
{
    public interface IObjectScopeGuard : IDisposable
    {
    }

    public class ObjectScopeGuard<T> : IObjectScopeGuard, IDisposable
    {
        private Action<T> _removeObject;
        private T _obj;
        private bool _disposed;

        public T Obj {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException("");
                return _obj;
            }
        }

        public ObjectScopeGuard(T obj, Action<T> removeObject)
        {
            _obj = obj;
            _removeObject = removeObject;
        }

        public ObjectScopeGuard(Func<T> createObject, Action<T> removeObject)
            :this(createObject(), removeObject)
        {
            _removeObject = removeObject;
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            
            if (_removeObject != null)
            {
                _removeObject.Invoke(_obj);
            }
            
            _obj = default(T);
            _disposed = true;
        }
    }
}
