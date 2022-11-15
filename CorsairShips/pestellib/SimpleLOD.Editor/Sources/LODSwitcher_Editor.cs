/* SimpleLOD 1.6     */
/* By Orbcreation BV */
/* Richard Knol      */
/* Mar 11, 2016      */

using System;
using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(LODSwitcher))]

public class LODSwitcher_Editor : Editor 
{
	private bool _updateLodWithSceneView = false;

	void OnEnable()
	{
		SceneView.onSceneGUIDelegate += OnSceneGui;
	}

	void OnDisable()
	{
		SceneView.onSceneGUIDelegate -= OnSceneGui;
	}
	
	public override void OnInspectorGUI () {
		LODSwitcher switcher = target as LODSwitcher;
		base.OnInspectorGUI(); // Show default GUI

		// And add a field to test the LOD level
		int oldLevel = switcher.GetLODLevel();
		if(!Application.isPlaying) {
			int newLevel = EditorGUILayout.IntSlider("LOD level", oldLevel, 0, switcher.MaxLODLevel());
			if(newLevel != oldLevel) {
				switcher.SetLODLevel(newLevel);
			}
		} else {
			EditorGUILayout.IntField("Currently using LOD level", oldLevel);
		}

		if (!Application.isPlaying)
		{
			_updateLodWithSceneView = EditorGUILayout.Toggle("Update LOD with scene camera", _updateLodWithSceneView);

			if (_updateLodWithSceneView)
			{
				(target as LODSwitcher).SetCustomCamera(SceneView.lastActiveSceneView.camera);
			}
			else
			{
				(target as LODSwitcher).SetCustomCamera(null);
				(target as LODSwitcher).SetLODLevel(0);
			}
		}
	}

	private void OnSceneGui(SceneView sceneView)
	{
		if (_updateLodWithSceneView)
		{
			(target as LODSwitcher).runInEditMode = true;
			(target as LODSwitcher).SetCustomCamera(SceneView.lastActiveSceneView.camera);
			(target as LODSwitcher).ForceUpdateLodLevel();
		}
	}
}