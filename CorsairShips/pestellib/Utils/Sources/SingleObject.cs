using UnityEngine;
using UnityDI;

namespace PestelLib.Utils
{
    public class SingleObject<T> : MonoBehaviour
    {
        [Dependency] private SingleObjectStack<T> stack;

        protected virtual void Awake()
        {
            ContainerHolder.Container.BuildUp(this);
            stack.Push(this);
        }

        protected virtual void OnDestroy()
        {
            stack.RemoveObject(this);
        }

        public virtual void Activate()
        {
            gameObject.SetActive(true);
        }

        public virtual void Deactivate()
        {
            gameObject.SetActive(false);
        }
    }
}