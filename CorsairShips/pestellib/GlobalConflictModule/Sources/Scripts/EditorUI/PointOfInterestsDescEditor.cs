using ServerShared;
using UnityEditor;
using UnityEngine;

namespace GlobalConflictModule.Scripts
{
    [CustomEditor(typeof(PointOfInterestsDesc))]
    public class PointOfInterestsDescEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var poi = target as PointOfInterestsDesc;
            var conflictEditor = poi.GetComponentInParent<GlobalConflictDesc>();

            if (conflictEditor.GeneralsCount < 1)
                EditorGUILayout.LabelField("To edit general lvl requirement, set generals count for the conflict", GUI.skin.box, GUILayout.ExpandWidth(true));
            else
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("General Level");
                    poi.GeneralLevel = EditorGUILayout.IntField(poi.GeneralLevel);
                }
            }

            if (!poi.ForAllTeams && conflictEditor.TeamIds != null)
            {
                var idx = conflictEditor.TeamIds.IndexOf(poi.Team);
                if (idx < 0)
                    idx = 0;
                idx = EditorGUILayout.Popup("Team", idx, conflictEditor.TeamIds);
                poi.Team = conflictEditor.TeamIds[idx];
            }

        }
    }
}
