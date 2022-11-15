using System.Collections.Generic;
using UnityDI;
using UnityEngine;

namespace PestelLib.ObjectsPool
{
    public class PoolManager : MonoBehaviour
    {
        [SerializeField] private List<ObjectsPool> _pools;

        private void Awake()
        {
            for (var i = 0; i < _pools.Count; i++)
            {
                _pools[i].InitPool(this);
            }
        }

        public GameObject Spawn(string poolId)
        {
            return Spawn(poolId, Vector3.zero, Quaternion.identity);
        }

        public GameObject Spawn(string poolId, Vector3 position, Quaternion rotation)
        {
            for (int i = 0; i < _pools.Count; i++)
            {
                if (_pools[i].Id == poolId)
                {
                    return _pools[i].Spawn(position, rotation);
                }
            }

            Debug.LogError("No pool with id " + poolId);
            return null;
        }

        public GameObject Spawn(GameObject prefab)
        {
            return Spawn(prefab, Vector3.zero, Quaternion.identity);
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            for (int i = 0; i < _pools.Count; i++)
            {
                if (_pools[i].Prefab == prefab)
                {
                    return _pools[i].Spawn(position, rotation);
                }
            }

            Debug.LogError("No pool with prefab " + prefab.name);
            return null;
        }

        public void Despawn(GameObject instance)
        {
            for (var i = 0; i < _pools.Count; i++)
            {
                if (_pools[i].Id == instance.name)
                {
                    _pools[i].Despawn(instance);
                }
            }
        }
    }
}