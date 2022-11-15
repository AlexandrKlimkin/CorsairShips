using System;
using System.IO;
using Modules.BundleLoader;
using UnityEditor;
#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEditor.SceneManagement;
using UnityEngine;

public class PrepareTextures
{
    private const string HdSuffix = "_hd";
    private const string MdSuffix = "_md";

    private const int LowResTextureSize = 128;
    private const int MiddleResTextureSize = 1024;

    [MenuItem("PestelCrew/AssetBundles2/Make HD textures from selected")]
    public static void MakeHDDuplicatesFromSelected()
    {
        var textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);
        foreach (var texture2D in textures)
        {
            var texturePath = AssetDatabase.GetAssetPath(texture2D);
            var shortName = Path.GetFileNameWithoutExtension(texturePath);
            if (shortName.EndsWith(HdSuffix)) continue;

            var directory = Path.GetDirectoryName(texturePath);

            var extension = Path.GetExtension(texturePath);

            var newPath = directory + Path.DirectorySeparatorChar + shortName + HdSuffix + extension;
            Debug.LogFormat("Copy from {0} to {1}", texturePath, newPath);
            AssetDatabase.CopyAsset(texturePath, newPath);

            var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

            var settings = importer.GetPlatformTextureSettings("Android");
            settings.maxTextureSize = LowResTextureSize;
            importer.SetPlatformTextureSettings(settings);

            settings = importer.GetDefaultPlatformTextureSettings();
            settings.maxTextureSize = LowResTextureSize;
            importer.SetPlatformTextureSettings(settings);

            importer.SaveAndReimport();
        }
    }

    [MenuItem("PestelCrew/AssetBundles2/Make MD textures from selected HD textures")]
    public static void MakeMDDuplicatesFromSelected()
    {
        var textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);
        foreach (var texture2D in textures)
        {
            var texturePath = AssetDatabase.GetAssetPath(texture2D);
            var shortName = Path.GetFileNameWithoutExtension(texturePath).Replace("_hd", "_md");
            var newPath = Path.GetDirectoryName(texturePath) + Path.DirectorySeparatorChar + shortName + Path.GetExtension(texturePath);
            AssetDatabase.CopyAsset(texturePath, newPath);
            Debug.Log(newPath);

            var importer = (TextureImporter)AssetImporter.GetAtPath(newPath);

            var settings = importer.GetPlatformTextureSettings("Android");
            settings.maxTextureSize = MiddleResTextureSize;
            importer.SetPlatformTextureSettings(settings);

            settings = importer.GetDefaultPlatformTextureSettings();
            settings.maxTextureSize = MiddleResTextureSize;
            importer.SetPlatformTextureSettings(settings);

            importer.SaveAndReimport();
        }
    }

    [MenuItem("PestelCrew/AssetBundles2/Debug: print textures from material")]
    public static void GetTextures()
    {
        var materials = Selection.GetFiltered<Material>(SelectionMode.Assets);
        foreach (var mat in materials)
        {
            ProcessMaterialTextures(mat, 
                (a, b) =>
                {
                    Debug.Log(a + " " + b);
                }
            );
        }
    }

    private static void ProcessMaterialTextures(Material mat, Action<string, string> processMaterialTexture)
    {
        for (var i = 0; i < ShaderUtil.GetPropertyCount(mat.shader); i++)
        {
            var propertyType = ShaderUtil.GetPropertyType(mat.shader, i);
            if (propertyType == ShaderUtil.ShaderPropertyType.TexEnv)
            {
                var texturePropertyName = ShaderUtil.GetPropertyName(mat.shader, i);
                var texture = mat.GetTexture(texturePropertyName);
                if (texture != null)
                {
                    
                    processMaterialTexture(texturePropertyName, texture.name);
                }
            }
        }
    }

    [MenuItem("PestelCrew/AssetBundles2/Clear asset bundles cache")]
    public static void ClearCache()
    {
        Caching.ClearCache();
    }

    [MenuItem("PestelCrew/AssetBundles2/Add texture bundle loaders")]
    public static void AddTextureBundleLoaders()
    {
        var gameObjects = Selection.GetFiltered<GameObject>(SelectionMode.Unfiltered);

        foreach (var selectedGo in gameObjects)
        {
            var renders = selectedGo.GetComponents<Renderer>();
            foreach (var r in renders)
            {
                var mat = r.sharedMaterial;
                ProcessMaterialTextures(mat, (texturePropertyName, textureName) =>
                    TryAddLoader(r.gameObject, texturePropertyName, textureName)
                );
            }
        }

#if UNITY_2018_3_OR_NEWER
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            EditorSceneManager.MarkSceneDirty(prefabStage.scene);
        }
#endif

        void TryAddLoader(GameObject go, string texturePropertyName, string assetName)
        {
            if (HaveNotAdded(go, texturePropertyName, assetName))
            {
                var loader = go.AddComponent<TextureBundleLoader>();
                loader.textureName = texturePropertyName;
                loader.assetName = assetName;
                loader.bundleName = go.transform.root.name.ToLowerInvariant();
            }
        }

        bool HaveNotAdded(GameObject go, string texturePropertyName, string assetName)
        {
            var loaders = go.GetComponents<TextureBundleLoader>();
            foreach (var textureBundleLoader in loaders)
            {
                if (textureBundleLoader.textureName == texturePropertyName &&
                    textureBundleLoader.assetName == assetName)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
