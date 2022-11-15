using System.Collections.Generic;
using UnityEngine;

namespace PestelLib.ObjectsPool
{
    [System.Serializable]
    public class ObjectsPool
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private int _poolSize;

        [HideInInspector] [SerializeField] private List<GameObject> _items;

        private PoolManager _manager;
        private Transform _poolObject;

        private int _currentIdx;

        public string Id
        {
            get { return _prefab.name; }
        }

        public GameObject Prefab
        {
            get { return _prefab; }
        }

        public void InitPool(PoolManager manager)
        {
            if (_manager != null)
            {
                Debug.LogError(Id + ": Pool already inited");
                return;
            }

            _manager = manager;

            if (_items == null)
                _items = new List<GameObject>();

            InstantiatePool();
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            GameObject result = GetObjectInReserve();

            if (result == null)
            {
                result = _items[_currentIdx];
                _currentIdx++;

                NotifyDespawn(result);

                if (_currentIdx >= _items.Count)
                    _currentIdx = 0;
            }

            result.SetActive(true);

            result.transform.parent = null;
            result.transform.position = position;
            result.transform.rotation = rotation;

            NotifySpawn(result);

            return result;
        }

        public void Despawn(GameObject go)
        {
            NotifyDespawn(go);

            go.SetActive(false);
            go.transform.SetParent(_poolObject, false);
        }

        private void InstantiatePool()
        {
            var pool = new GameObject(Id);
            _poolObject = pool.transform;

            _poolObject.SetParent(_manager.transform, false);

            for (int i = 0; i < _poolSize; i++)
            {
                var obj = (GameObject)Object.Instantiate(Prefab, _poolObject.position, _poolObject.rotation);
                obj.name = Id;
                obj.transform.SetParent(_poolObject);

                obj.SetActive(false);

                _items.Add(obj);

                foreach (var poolable in obj.GetComponents<IPoolableObject>())
                {
                    poolable.OnPlacedInPool();
                }
            }
        }

        private bool IsInPool(GameObject go)
        {
            return go.transform.parent == _poolObject;
        }

        private GameObject GetObjectInReserve()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (IsInPool(_items[i]))
                    return _items[i];
            }

            return null;
        }

        private void NotifySpawn(GameObject go)
        {
            foreach (var poolable in go.GetComponents<IPoolableObject>())
            {
                poolable.OnSpawn();
            }
        }

        private void NotifyDespawn(GameObject go)
        {
            foreach (var poolable in go.GetComponents<IPoolableObject>())
            {
                poolable.OnDespawn();
            }
        }
    }
}