using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class MonoBehaviourPool<T> where T : MonoBehaviour
    {
        public T template;
        public Transform parent;

        protected List<T> objects;

        protected List<T> activeObjects;
        public IReadOnlyList<T> ActiveObjects => activeObjects;


        public MonoBehaviourPool(T template, Transform parent)
        {
            objects = new List<T>();
            this.template = template;
            this.parent = parent;
            parent.GetComponentsInChildren(objects);
            activeObjects = objects.Where(_=>_.gameObject.activeSelf).ToList();
        }

        public T GetObject()
        {
            var obj = objects.FirstOrDefault(_ => !_.gameObject.activeSelf);
            if (obj == null)
            {
                obj = Object.Instantiate(template, parent);
                objects.Add(obj);
            }

            obj.gameObject.SetActive(true);
            activeObjects.Add(obj);
            
            return obj;
        }

        public void ReturnObjectToPool(T obj)
        {
            obj.gameObject.SetActive(false);
            activeObjects.Remove(obj);
        }

        public void ReturnAllToPool() {
            objects.ForEach(ReturnObjectToPool);
        }
    }
}
