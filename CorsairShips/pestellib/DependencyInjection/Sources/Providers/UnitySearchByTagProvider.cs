using UnityEngine;

namespace UnityDI.Providers
{
    public class UnitySearchByTagProvider<T> : IObjectProvider<T> where T : Component
    {
        private readonly string _tag;

        public UnitySearchByTagProvider(string tag)
        {
            _tag = tag;
        }

        public T GetObject(Container container)
        {
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(_tag);
            //When you changing prefab in Project View and don't hit Save Project after - ghost object will appear)
            //This object has same tag, name and other stuff, but it's components will be null.
            //So we have to check all objects with tag to prevent errors

            if (gameObjects == null || gameObjects.Length == 0)
                throw new ContainerException("Can't find game object with tag \"" + _tag + "\"");

            //Debug.Log("Found object with tag " + gameObject.tag + " name '" + gameObject.name + "' and instance id " + gameObject.GetInstanceID() + " it has " + gameObject.GetComponents(typeof(MonoBehaviour)).Length + " components");

            int objectsCount = gameObjects.Length;
            for (int i = 0; i < objectsCount; i++)
            {
                if (objectsCount > 1)
                {
                    var components = gameObjects[i].GetComponents<Component>();
                    if (components.Length <= 1) continue; //Prevent ghost player
                }

                T component = gameObjects[i].GetComponent<T>();
                if (component != null)
                    return component;
            }

            throw new ContainerException("Can't find component \"" + typeof(T).FullName +
                                         "\" of game object with tag \"" + _tag + "\"");
        }
    }
}