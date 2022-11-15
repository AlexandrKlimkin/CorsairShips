using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class SceneOrganizerBase : MonoBehaviour
{
#if UNITY_EDITOR

	[SerializeField] ContainerData[] datas;

	private Dictionary<System.Type, Transform> containers;

    protected virtual void Awake()
    {
		containers = new Dictionary<System.Type, Transform>();

        foreach (var data in datas)
        {
			var type = data.componentReference?.GetType();
			if (type is null)
				continue;

			var container = CreateContainer(type, data.containerName);

			if (data.checkExisting)
			{
				var objects = GameObject.FindObjectsOfType(type, true).Cast<Component>();
				foreach (var obj in objects)
					if (obj.transform.parent != container)
						obj.transform.SetParent(container);
			}
		}
	}

    protected Transform CreateContainer(System.Type type, string name)
	{
		var container = GameObject.Find(name)?.transform;

		if (container == null)
			container = new GameObject(name).transform;

		containers.Add(type, container);

		return container;
	}

	public void Store(Component @object)
    {
		var type = @object.GetType();

		foreach (var pair in containers)
			if (pair.Key == type || type.IsSubclassOf(pair.Key))
				@object.transform.SetParent(pair.Value);
			//else
			//	Debug.Log($"Could not identify container for {@object.name} ({type.Name})");
	}

	[System.Serializable]
	public struct ContainerData
    {
		public Component componentReference;
		public string containerName;
		public bool checkExisting;
	}

#endif
}