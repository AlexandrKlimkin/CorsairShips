using UnityEngine;
using UnityEditor;

public class UserProfileWindow : EditorWindow {

	[MenuItem("PestelCrew/SharedLogic/Profile Viewer")]
	static void Init() {
	    var window = (UserProfileWindow)EditorWindow.GetWindow(typeof(UserProfileWindow));
	    window.titleContent = new GUIContent("Profile Viewer");
	    window.Show();
	}

	UserProfileViewer profileViewer;
	Editor editor;
	Vector2 scroll;

	void OnDisable() {
		profileViewer = null;
		editor = null;
	}

	void OnGUI() {
		if (profileViewer == null)
		{
			profileViewer = GameObject.FindObjectOfType<UserProfileViewer>();
            if (profileViewer == null)
            {
                var go = new GameObject("UserProfileViewer");
                profileViewer = go.AddComponent<UserProfileViewer>();
            }
		    editor = Editor.CreateEditor(profileViewer);
		}		

		if (editor) {
			scroll = EditorGUILayout.BeginScrollView(scroll);
		    editor.OnInspectorGUI();            
		    EditorGUILayout.EndScrollView();
		}
	}
}
