using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace PestelLib.UI
{
    [RequireComponent(typeof(RawImage))]
    public class RawImageLoader : MonoBehaviour
    {
        [SerializeField] private string _assetName;
        private RawImage rawImage;

        public event Action OnImageLoaded = () => { };

        [Space]
        [Header("Use it for more useful editing in black background for example...")]
        [SerializeField] private bool _isUseDefaultColor = true;
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private Color _colorBeforeLoad = Color.black;

        private void Awake()
        {
            rawImage = GetComponent<RawImage>();
            LoadTexture();       
        }

        private void OnEnable()
        {
            LoadTexture();
        }

        private void LoadTexture()
        {
                           
            if (rawImage.texture != null)
                return;
            if (_isUseDefaultColor)
            {
                rawImage.color = _colorBeforeLoad;
            }
            if (TexturePreloader.Instance != null)
            {
                var texutre = TexturePreloader.Instance.GetTexture(_assetName);
                if (texutre != null)
                {
                    rawImage.texture = texutre;
                    OnImageLoaded.Invoke();
                    if (_isUseDefaultColor)
                    {
                        rawImage.color = _defaultColor;
                    }
                    return;
                }
            }

            StartCoroutine(LoadTextureCoroutine(_assetName));
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

                rawImage.texture = www.texture;
                if (_isUseDefaultColor)
                {
                    rawImage.color = _defaultColor;
                }
                OnImageLoaded.Invoke();

            }

            yield return 0;
        }
    }
}

