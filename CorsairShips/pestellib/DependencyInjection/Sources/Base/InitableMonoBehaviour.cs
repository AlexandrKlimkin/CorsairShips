using System.Collections.Generic;
using System.Linq;
using UnityDI.Base;
using UnityEngine;

namespace UnityDI
{ 
	public class InitableMonoBehaviour : MonoBehaviour, IInitable {
        protected bool Initialized { get; set; }

	    public int Priority
	    {
	        get { return _priority; }
            set { _priority = value; }
	    }
	    [SerializeField] private int _priority;

	    public void OnInitDone()
        {
            if (Initialized) return;
            Initialized = true;
            ContainerHolder.Container.BuildUp(GetType(), this);
            SafeStart();
        }

	    virtual protected void Start()
	    {
            if (GameInitState.InitComplete)
	        {
                OnInitDone();
	        }
	    }

		private void Update () {
            if (Initialized)
            {
                SafeUpdate();
            }		
		}

	    protected virtual void SafeStart()
        {
	    }

	    public virtual void SafeUpdate()
	    {
	    }

        public static void Initialize(GameObject obj, bool recursive)
	    {
            var allInitable = new List<IInitable>();
            allInitable.AddRange(obj.GetComponents<IInitable>());
            
            if (recursive)
            {
                allInitable.AddRange(obj.GetComponentsInChildren<IInitable>());
            }

            var sortedInitable = allInitable.OrderBy(x => x.Priority);
	        foreach (var initable in sortedInitable)
	        {
	            initable.OnInitDone();
	        }
	    }
	}
}