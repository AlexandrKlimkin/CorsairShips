using System;
using System.Collections.Generic;
using System.Linq;
using UnityDI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PestelLib.UI
{
    //Класс для последовательного отображения окон. Здесь окна не накладываются друг на друга, а открываются в порядке очереди (Queue) после закрытия предыдущего окна
    public class NotificationGui : MonoBehaviour
    {
        protected readonly Dictionary<Type, Queue<GameObject>> _dict = new Dictionary<Type, Queue<GameObject>>(); 

        [Dependency] protected Gui _gui;

        protected GameObject _activeScreen;

        protected virtual void Awake()
        {
            ContainerHolder.Container.BuildUp(this);
        }

        public T[] GetQueued<T>() where T: Component
        {
            Queue<GameObject> q;
            if(!_dict.TryGetValue(typeof(T), out q))
                return new T[] {};

            var current = _activeScreen != null ? _activeScreen.GetComponent<T>() : null;
            return q.Where(_ => _ != null)
                .Select(_ => _.GetComponent<T>())
                .Concat(new [] { current })
                .Where(_ => _ != null).ToArray();
        }

        public T Show<T>() where T : Component
        {
            return Show(typeof(T)).GetComponent<T>();
        }

        public void Close<T>(T screen) where T : Component
        {
            if (_activeScreen == screen.gameObject)
            {
                _activeScreen = null;
            }
            Destroy(screen.gameObject);
            ShowNext(typeof(T));
        }

        protected GameObject Show(Type screenClass)
        {
            GameObject screen = CreateScreen(screenClass);

            if (_dict.ContainsKey(screenClass))
            {
                _dict[screenClass].Enqueue(screen);
                return screen;
            }

            Queue<GameObject> queue = new Queue<GameObject>();
            queue.Enqueue(screen);

            _dict[screenClass] = queue;

            ShowNext(screenClass);

            return screen;
        }

        protected virtual void ShowNext(Type screenClass)
        {
            if (_dict.ContainsKey(screenClass))
            {
                if (_dict[screenClass].Count > 0)
                {
                    if (_activeScreen != null) return;

                    _activeScreen = _dict[screenClass].Dequeue();
                    _activeScreen.SetActive(true);
                    _activeScreen.transform.SetAsLastSibling();
                    _gui.PopUpBackgroundInstance.SetActive(true);
                    _gui.PopUpBackgroundInstance.transform.SetAsLastSibling();
                    return;
                }
                _dict.Remove(screenClass);
                _gui.PopUpBackgroundInstance.SetActive(false);
                _gui.PopUpBackgroundInstance.transform.SetSiblingIndex(0);
            }

            if (_dict.Count > 0)
            {
                foreach (KeyValuePair<Type, Queue<GameObject>> pair in _dict)
                {
                    ShowNext(pair.Key);
                    break;
                }
            }
        }

        protected GameObject CreateScreen(Type screenClass)
        {
            string screenPrefabName = screenClass.Name;
            Object screenObject = Resources.Load("Screens/" + screenPrefabName);
            if (screenObject == null)
            {
                Debug.LogError("Cannot find screen " + screenPrefabName);
                return null;
            }

            var screen = Instantiate(screenObject) as GameObject;
            screen.transform.SetParent(_gui.PopUpCanvas.transform, false);
            screen.name = screenPrefabName;
            screen.SetActive(false);
            return screen;
        }

    }
}
