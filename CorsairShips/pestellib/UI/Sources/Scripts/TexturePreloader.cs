using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PestelLib.UI
{
    public class TexturePreloader : MonoBehaviour
    {
        [SerializeField] private string[] _assetNames;

        public static TexturePreloader Instance { get; private set; }

        public Dictionary<string, Texture> TexturesCache { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            TexturesCache = new Dictionary<string, Texture>();

            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            Preload();
        }

        private void Preload()
        {
            StartCoroutine(PreloadCoroutine());
        }

        private IEnumerator PreloadCoroutine()
        {
            foreach (var assetName in _assetNames)
            {
                yield return StartCoroutine(LoadTextureCoroutine(assetName));
            }
        }

        public Texture GetTexture(string assetName)
        {
            if (TexturesCache.ContainsKey(assetName))
            {
                return TexturesCache[assetName];
            }

            return null;
        }

        private IEnumerator LoadTextureCoroutine(string assetName)
        {
            var path = "";
#if UNITY_EDITOR
            path = "file://" + Path.Combine(Application.streamingAssetsPath, assetName);
#elif UNITY_ANDROID
            path = "jar:file://" + Path.Combine(Application.dataPath + "!/assets/", assetName);
#endif
            WWW www = new WWW(path);

            while (!www.isDone)
            {

                yield return null;
            }

            if (!string.IsNullOrEmpty(www.error))
            {

                Debug.LogError(www.error);
                yield break;
            }
            else
            {

                TexturesCache.Add(assetName, www.texture);
            }

            yield return 0;
        }
    }

}

