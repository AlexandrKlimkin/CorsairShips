using System;
using System.Collections;
using UnityEngine;
using UnityDI;

namespace PestelLib.AssetBundles
{
    public class TextureLoader : MonoBehaviour
    {
        public Action<TextureLoader> TexturesLoadingFinished = (x) => { };

        private const string TEXTURES_POSTFIX = "_hd";

        private string _bundleName;
        private Material _material;

        [Dependency]
        private AssetBundleManager _assetBundleManager;

        public string ShipName
        {
            set
            {
                _bundleName = string.Format("{0}/{1}", BundlesLoadingController.SHIP_BUNDLES_GROUP, value.ToLower());
                BundlesLoadingController.LoadingFinished += OnBundleLoaded;
            }
        }

        void Awake()
        {
            ContainerHolder.Container.BuildUp(this);
        }

        public void SetMaterialTexture(Material material)
        {
            if (string.IsNullOrEmpty(_bundleName))
            {
                Debug.LogError("Set shipName before loading texture bundle!");
                return;
            }

            _material = material;

            BundlesLoadingController.Instance.AddDownload(_bundleName);
        }


        private void OnBundleLoaded(string loadedBundleName, bool success)
        {
            if (string.Equals(loadedBundleName, _bundleName))
            {
                if (success)
                    StartCoroutine(LoadTextures());
            }
        }

        private IEnumerator LoadTextures()
        {
            var bundle = _assetBundleManager.GetBundle(_bundleName);

            var mainTextureName = _material.mainTexture.name + TEXTURES_POSTFIX;
            var request = bundle.LoadAssetAsync(mainTextureName, typeof(Texture2D));
            while (!request.isDone)
                yield return null;
            var texture = request.asset as Texture2D;
            if (texture != null)
                _material.mainTexture = texture;


            var secTextureName = _material.GetTexture("_MetalGlossMap").name + TEXTURES_POSTFIX;
            request = bundle.LoadAssetAsync(secTextureName, typeof(Texture2D));
            while (!request.isDone)
                yield return null;

            texture = request.asset as Texture2D;
            if (texture != null)
                _material.SetTexture("_MetalGlossMap", texture);

            TexturesLoadingFinished(this);
        }

        private void OnDisable()
        {
            TexturesLoadingFinished(this);
            BundlesLoadingController.LoadingFinished -= OnBundleLoaded;
        }
    }
}