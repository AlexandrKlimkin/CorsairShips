using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using PestelLib.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ListSpriteAtlas {
    private const string StringPreferenceKey = "BoolPreferenceKey";

    [MenuItem("PestelCrew/UI/Clean raycast target flag")]
    public static void CleanRaycastTarget()
    {
        Debug.Log(Selection.activeGameObject);

        var images = Selection.activeGameObject.GetComponentsInChildren<Image>(true);

        foreach (var image in images)
        {
            var culture = CultureInfo.InvariantCulture;
            var containsButton = culture.CompareInfo.IndexOf(image.gameObject.name, "button", CompareOptions.IgnoreCase) >= 0;
            var containsBtn = culture.CompareInfo.IndexOf(image.gameObject.name, "btn", CompareOptions.IgnoreCase) >= 0;
            var containsTouch = culture.CompareInfo.IndexOf(image.gameObject.name, "touch", CompareOptions.IgnoreCase) >= 0;

            image.raycastTarget = containsButton || containsTouch || containsBtn;
        }
    }

    [MenuItem("PestelCrew/UI/List sprite atlases")]
	public static void ListAtlases()
	{
	    HashSet<Sprite> sprites = new HashSet<Sprite>();
        
        StringBuilder report = new StringBuilder();

        Debug.Log(Selection.activeGameObject);

	    var images = Selection.activeGameObject.GetComponentsInChildren<Image>(true);

	    foreach (var image in images)
	    {
	        if (image.sprite != null && !sprites.Contains(image.sprite))
	        {
	            sprites.Add(image.sprite);
	            var fullpath = AssetDatabase.GetAssetPath(image.sprite.texture);
                report.AppendFormat("{0,-40}; {1, -60}; {2, -100},\r\n", image.sprite.name, image.sprite.texture, fullpath);
	        }
	    }
        
        Debug.Log(report);
        Debug.Log("Total sprites: " + sprites.Count);
	}

    [MenuItem("PestelCrew/UI/Move sprites from selected GO (and its children) to HUD directory (see editor preferences)")]
    public static void MoveSprites()
    {
        HashSet<Sprite> sprites = new HashSet<Sprite>();

        List<string> spritesPaths = new List<string>();

        Debug.Log(Selection.activeGameObject);

        var images = Selection.activeGameObject.GetComponentsInChildren<Image>(true);

        foreach (var image in images)
        {
            if (image.sprite != null && !sprites.Contains(image.sprite))
            {
                sprites.Add(image.sprite);
                var fullpath = AssetDatabase.GetAssetPath(image.sprite.texture);

                var newDir = pathPreference + Path.GetFileName(fullpath);
                var oldDir = Path.GetDirectoryName(fullpath) + "/" + Path.GetFileName(fullpath);

                if (newDir != oldDir)
                {
                    spritesPaths.Add(oldDir);
                }
            }
        }
        
        var move = EditorUtility.DisplayDialog(
            "Moving sprites", 
            "Are you sure to move those sprites: " + string.Join("\n", spritesPaths.ToArray().Take(Mathf.Max(spritesPaths.Count, 40)).ToArray()) + "... and some others to folder " + pathPreference + " ?",
            "Yes", "No"
        );

        if (move)
        {
            foreach (var path in spritesPaths)
            {
                var newDir = pathPreference + Path.GetFileName(path);
                AssetDatabase.MoveAsset(path, newDir);
            }
            AssetDatabase.Refresh();
        }
    }

    [MenuItem("PestelCrew/UI/List 'null' sprites in selected GO and its children")]
    public static void ListNullSprites()
    {
        var images = Selection.gameObjects[0].GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            if (img.sprite == null)
            {
                Debug.Log(img.transform.FullPath());
            }
        }
    }

    [MenuItem("PestelCrew/UI/Replace 'null' sprite to selected sprite in project")]
    public static void ReplaceNullSprites()
    {
        Sprite sprite;
        var allSprites = Selection.GetFiltered(typeof(Sprite), SelectionMode.Assets);
        if (allSprites.Length == 0)
        {
            var selectedTextures = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);
            if (selectedTextures.Length != 1)
            {
                EditorUtility.DisplayDialog("Error", "Please select sprite in project window and try again", "OK");
                return;
            }

            string spriteSheet = AssetDatabase.GetAssetPath(selectedTextures[0]);
            var sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheet).OfType<Sprite>().ToArray();
            if (sprites.Length != 1)
            {
                EditorUtility.DisplayDialog("Error", "Please select sprite in project window and try again", "OK");
                return;
            }

            sprite = sprites[0];
        }
        else
        {
            sprite = allSprites[0] as Sprite;
        }

        if (Selection.gameObjects.Length != 1)
        {
            EditorUtility.DisplayDialog("Error", "Please select root game object in hierarchy or scene view", "OK");
            return;
        }

        var images = Selection.gameObjects[0].GetComponentsInChildren<Image>(true);

        foreach (var img in images)
        {
            if (img.sprite == null)
            {
                img.sprite = sprite;
            }
        }
    }
    
    private static Color defaultMaskColor = new Color(1f, 1f, 1f, 0f);
    [MenuItem("PestelCrew/UI/Replace default unity sprite to selected sprite in project")]
    public static void ReplaceDefaultSprites()
    {
        Sprite sprite;
        var allSprites = Selection.GetFiltered(typeof(Sprite), SelectionMode.Assets);
        if (allSprites.Length == 0)
        {
            var selectedTextures = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);
            if (selectedTextures.Length != 1)
            {
                EditorUtility.DisplayDialog("Error", "Please select sprite in project window and try again", "OK");
                return;
            }

            string spriteSheet = AssetDatabase.GetAssetPath(selectedTextures[0]);
            var sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheet).OfType<Sprite>().ToArray();
            if (sprites.Length != 1)
            {
                EditorUtility.DisplayDialog("Error", "Please select sprite in project window and try again", "OK");
                return;
            }

            sprite = sprites[0];
        }
        else
        {
            sprite = allSprites[0] as Sprite;
        }

        if (Selection.gameObjects.Length != 1)
        {
            EditorUtility.DisplayDialog("Error", "Please select root game object in hierarchy or scene view", "OK");
            return;
        }

        var images = Selection.gameObjects[0].GetComponentsInChildren<Image>(true);

        foreach (var img in images)
        {
            if (img.sprite.name == "UIMask" || img.sprite.name == "Background" || img.sprite.name == "UISprite")
            {
                img.sprite = sprite;
                img.color = defaultMaskColor;
                img.type = Image.Type.Simple;
            }
        }
    }
    
    [MenuItem("PestelCrew/UI/Replace 'Mask' to 'RectMask2D")]
    public static void ReplaceMask()
    {
        if (Selection.gameObjects.Length != 1)
        {
            EditorUtility.DisplayDialog("Error", "Please select root game object in hierarchy or scene view", "OK");
            return;
        }
        
        var masks = Selection.gameObjects[0].GetComponentsInChildren<Mask>(true);

        foreach (var mask in masks)
        {
            mask.gameObject.AddComponent<RectMask2D>();
            GameObject.DestroyImmediate(mask);
        }
    }

    // Have we loaded the prefs yet
    private static bool prefsLoaded = false;

    // The Preferences
    public static string pathPreference = "Assets/DesertWars/ui/ui_art/hud/";

    // Add preferences section named "My Preferences" to the Preferences Window
    [PreferenceItem("Pestel Crew")]
    public static void PreferencesGUI()
    {
        // Load the preferences
        if (!prefsLoaded)
        {
            pathPreference = EditorPrefs.GetString(StringPreferenceKey, pathPreference);
            prefsLoaded = true;
        }

        // Preferences GUI
        pathPreference = EditorGUILayout.TextField("HUD folder", pathPreference);

        // Save the preferences
        if (GUI.changed)
            EditorPrefs.SetString(StringPreferenceKey, pathPreference);
    }
}
