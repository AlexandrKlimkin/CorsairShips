using System;
using System.Collections.Generic;
using UnityEngine;

namespace PestelLib.Utils
{
    public class SingleObjectStack<T>
    {
        public Action<SingleObject<T>, bool> OnObjectSwitched = (c, b) => { };

        [SerializeField] private readonly List<SingleObject<T>> _objects = new List<SingleObject<T>>();

        public void Push(SingleObject<T> obj)
        {
            if (obj == null)
            {
                Debug.LogError("Can't push null to objects stack");
                return;
            }

            if (_objects.Contains(obj))
            {
                _objects.Remove(obj);
            }

            SetActiveObjectState(false);
            _objects.Add(obj);
            SetActiveObjectState(true);
        }

        private void Pop()
        {
            if (_objects.Count < 2)
            {
                return;
            }

            SetActiveObjectState(false);
            _objects.RemoveAt(_objects.Count - 1);
            SetActiveObjectState(true);
        }

        public void RemoveObject(SingleObject<T> obj)
        {
            var index = _objects.IndexOf(obj);
            if (index == _objects.Count - 1)
            {
                Pop();
            }
            else if (index != -1)
            {
                _objects.RemoveAt(index);
            }
        }

        public SingleObject<T> TopObject
        {
            get { return _objects.Count > 0 ? _objects[_objects.Count - 1] : null; }
        }

        private void SetActiveObjectState(bool state)
        {
            if (TopObject != null)
            {
                OnObjectSwitched(TopObject, state);
                if (state)
                {
                    TopObject.Activate();
                }
                else
                {
                    TopObject.Deactivate();
                }
            }
        }
    }
}
