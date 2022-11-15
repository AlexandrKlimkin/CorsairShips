using UnityEngine;
using UnityEngine.UI;
using System;
using PestelLib.Utils;

namespace PestelLib.UI
{
    public class Widget3dSpawner : MonoBehaviour
    {
        public Action OnWidgetSpawned = () => { };
        public Action OnWidgetDestroyed = () => { };
        public Transform target;
        public AlignToWorldObject prototype;
        public GameObject widgetGameObj;
        public AlignToWorldObject Widget { get; protected set; }
        public bool IsInteractive;
        public bool UseTagRegistry;

        private void Start()
        {
            Spawn();
        }

        public void ForceSpawn()
        {
            Spawn();
        }

        public void ForceSpawn(Transform container)
        {
            Spawn(container);
        }

        public void ForceDestroy()
        {
            if (Widget != null && Widget.gameObject != null)
            {
                Destroy(Widget.gameObject);
            }
            OnWidgetDestroyed();
        }

        private void Spawn()
        {
            if (Widget != null) return;

            var panelTag = (IsInteractive) ? "HudLabelsInteractive" : "HudLabels";
            var container = UseTagRegistry
                ? TagRegistry.GetObjectByTag<Transform>(panelTag)
                : GameObject.FindGameObjectWithTag(panelTag).transform;

            Spawn(container);
        }

        private void Spawn(Transform container)
        {
            if (Widget != null) return;

            Widget = Instantiate(prototype);
            Widget.transform.SetParent(container, false);

            widgetGameObj = Widget.gameObject;

            Widget.Target = target;

            ProcessWidget();

            OnWidgetSpawned();
        }

        protected virtual void ProcessWidget()
        {
            
        }

        private void OnDestroy()
        {
            if (Widget != null && Widget.gameObject != null)
            {
                Destroy(Widget.gameObject);
            }
            OnWidgetDestroyed();
        }
    }
}