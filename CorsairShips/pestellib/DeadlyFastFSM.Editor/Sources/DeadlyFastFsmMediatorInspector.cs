using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PestelLib.DeadlyFastFSM.Editor
{
    [CustomEditor(typeof(DeadlyFastFsmMediator))]
    public class DeadlyFastFsmMediatorInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var mediator = (DeadlyFastFsmMediator)target;

            if (!Application.isPlaying && GUILayout.Button("Add event"))
            {
                mediator.Events.Add(new DeadlyFastFsmMediator.StoredEvent());
            }

            List<DeadlyFastFsmMediator.StoredEvent> destroyList = new List<DeadlyFastFsmMediator.StoredEvent>();

            foreach (var evt in mediator.Events)
            {
                GUILayout.BeginHorizontal();

                if (!Application.isPlaying)
                {
                    if (GUILayout.Button("Select Event"))
                    {
                        DeadlyFastFsmMediator.StoredEvent evt1 = evt;
                        Action<EventInfo> selected = e =>
                        {
                            evt1.TargetType = e.DeclaringType;
                            evt1.EventName = e.Name;
                            EditorUtility.SetDirty(mediator);
                        };

                        ShowGameObjectEventSelectionMenu(mediator.gameObject, null, selected);
                    }

                    if (GUILayout.Button("X"))
                    {
                        destroyList.Add(evt);
                    }
                }

                GUILayout.EndHorizontal();

                if (evt.TargetType != null)
                {
                    GUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("Selected Type", evt.TargetType.Name);
                    EditorGUILayout.LabelField("Selected Event", evt.EventName);
                    GUILayout.EndVertical();
                }
            }

            foreach (var evt in destroyList)
            {
                mediator.Events.RemoveAt(mediator.Events.IndexOf(evt));
                EditorUtility.SetDirty(mediator);
            }
        }

        public static void ShowGameObjectEventSelectionMenu(GameObject go, Type argType, Action<EventInfo> callback)
        {
            var menu = new GenericMenu();
            foreach (var comp in go.GetComponents(typeof(Component))/*.Where(c => c.hideFlags == 0)*/)
                menu = GetEventSelectionMenu(comp.GetType(), argType, callback, menu);
            menu.ShowAsContext();
            Event.current.Use();
        }

        ///Get a GenericMenu for Events of the type and only event handler type of System.Action
        public static GenericMenu GetEventSelectionMenu(Type type, Type argType, Action<EventInfo> callback, GenericMenu menu = null, string subMenu = null)
        {

            if (menu == null)
                menu = new GenericMenu();

            if (subMenu != null)
                subMenu = subMenu + "/";

            GenericMenu.MenuFunction2 selected = delegate(object selectedEvent)
            {
                callback((EventInfo)selectedEvent);
            };

            var itemAdded = false;
            var eventType = argType == null ? typeof(Action) : typeof(Action<>).MakeGenericType(new [] { argType });
            foreach (var e in type.GetEvents(BindingFlags.Instance | BindingFlags.Public))
            {
                if (e.EventHandlerType == eventType)
                {
                    var eventInfoString = string.Format("{0}({1})", e.Name, argType != null ? argType.Name : "");
                    menu.AddItem(new GUIContent(subMenu + type.Name + "/" + eventInfoString), false, selected, e);
                    itemAdded = true;
                }
            }

            if (!itemAdded)
                menu.AddDisabledItem(new GUIContent(subMenu + type.Name));

            return menu;
        }
    }
}
