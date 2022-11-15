using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityDI;

namespace Modules.BundleLoader
{
    public class TextureBundleLoader : BundleLoaderBase
    {
        [Dependency] private BundleManager Manager;

        protected override Object Resource { get; set; }

        [SerializeField] private string loadedTexturePostfix = "_hd";
        [SerializeField] private string loadedHalfResTexturePostfix = "_md";
        public string textureName = "_MainTex";

        protected Renderer meshRenderer;
        private Texture texture;

        [SerializeField] protected int materialIndex;

        private bool _alreadyModifiedMaterial = false;
        private bool _defaultLoadingStarted = false;
        private Coroutine loadingRoutine;

        private int MemorySizeNeeded = 1500;
        private bool IsLowDevice;
        public bool AllowBundleForLowMemoryDevices;
        public bool ForceLoadingHalfResTextures;

        private void Awake()
        {
            ContainerHolder.Container.BuildUp(this);
            IsLowDevice = SystemInfo.systemMemorySize < MemorySizeNeeded ? true : false;
        }

        private void OnEnable()
        {
            Manager.OnBundlesCleanUp += CleanUpBundlesCache;

            if (!_defaultLoadingStarted || _alreadyModifiedMaterial)
                return;
            
            GetReferences();
            LoadResource();
        }

        private void OnDisable()
        {
            Manager.OnBundlesCleanUp -= CleanUpBundlesCache;
        }

        protected override void GetReferences()
        {          
            meshRenderer = GetComponent<Renderer>();
            if (meshRenderer == null)
                return;

            //строим имя, чтобы найти текстуру с тем-же именем что и базовя, только с префиксом
            if (materialIndex >= meshRenderer.sharedMaterials.Length)
                return;

            var material = meshRenderer.sharedMaterials[materialIndex];
            if (material == null)
                return;

            var baseTexture = material.GetTexture(textureName);
            if (baseTexture == null)
                return;

            var baseTextureName = baseTexture.name;
            if (string.IsNullOrEmpty(baseTextureName))
                return;

            _alreadyModifiedMaterial = baseTextureName.Contains(loadedTexturePostfix) || baseTextureName.Contains(loadedHalfResTexturePostfix);

            var strBuilder = new StringBuilder();
            strBuilder.Append(baseTextureName);
            if (AllowBundleForLowMemoryDevices && IsLowDevice || ForceLoadingHalfResTextures)
            {
                strBuilder.Append(loadedHalfResTexturePostfix);
                Debug.Log("Loading halfres textures from bundles");
            }
            else
            {
                strBuilder.Append(loadedTexturePostfix);
            }            
            this.assetName = strBuilder.ToString(); //меняем имя, по которому будет искаться ассет
        }

        public override void LoadResource()
        {
            if (_alreadyModifiedMaterial)
                return;

            if (string.IsNullOrEmpty(AssetName))
                return;

            _defaultLoadingStarted = true;

            if (!bundlesCount.ContainsKey(AssetName))
                bundlesCount.Add(AssetName, 0);

            bundlesCount[AssetName]++;

            Object texture = null;

            if (AssetsCache.TryGetValue(AssetName, out texture))
            {
                if (texture != null)
                {
                    Resource = texture;
                    ApplyTexture();
                }
                else
                {
                    if (loadingRoutine == null)
                    {
                        loadingRoutine = StartCoroutine(LoadingProcessCoroutine());
                    }
                }
            }
            else
            {
                if (loadingRoutine == null)
                {
                    loadingRoutine = StartCoroutine(LoadingProcessCoroutine());
                }
            }
        }

        protected override IEnumerator LoadingProcessCoroutine()
        {
            yield return StartCoroutine(LoadResourceCoroutine<Texture>());

            if (Resource == null)
            {
                Debug.LogError(string.Format("{0} can't load {1} ! is NULL!", gameObject.name, assetName));
            }

            ApplyTexture();
        }

        private void ApplyTexture()
        {
            if (_alreadyModifiedMaterial) return;

            texture = (Texture) Resource; 
            meshRenderer.materials[materialIndex].SetTexture(textureName, texture);
        }

        private void CleanUpBundlesCache()
        {
            AssetsCache.Clear();
            bundlesCount.Clear();
        }

        private void OnDestroy()
        {
            if (bundlesCount.ContainsKey(AssetName))
                RemoveBundleIfAvailable();
        }

        private void RemoveBundleIfAvailable()
        {
            bundlesCount[AssetName]--;

            if (bundlesCount[AssetName] == 0 && Manager.needCleanUpAsserts)
                CleanUpBundle();
        }

        private void CleanUpBundle()
        {
            Resources.UnloadAsset(texture);
            AssetsCache.Remove(AssetName);
            bundlesCount.Remove(AssetName);
        }
    }
}

