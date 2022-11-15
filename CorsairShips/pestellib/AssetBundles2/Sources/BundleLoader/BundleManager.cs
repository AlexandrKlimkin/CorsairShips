using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Text;
using PestelLib.ClientConfig;
using UnityDI;

namespace Modules.BundleLoader
{

    public class BundleManager : MonoBehaviour
    {
        [Dependency] private Config _config;

        //public static readonly string ROOT_URL = "http://localhost:3000/unityBundles/";
        private string ROOT_URL;
        public static readonly string FILE_EXTENSION = ".unity3d";

        public static BundleManager Instance { get; private set; }

        /// <summary>
        /// Вызывается при добавлении нового загрузчика (например загрузчик текстур и прочие наследники)
        /// </summary>
        public event Action<BundleLoaderBase> OnLoaderAddedEvent = (bundleLoader) => { };

        /// <summary>
        /// Вызывается при загрузке любого из бандлов 
        /// </summary>
        public event Action<string, AssetBundle> OnBundleLoadedEvent = (t, b) => { };
        /// <summary>
        /// Вызывается при выгрузке всех бандлов
        /// </summary>
        public event Action OnBundlesCleanUp = () => { };

        [SerializeField] private List<string> bundlesNamesToLoad = new List<string>();
        private Dictionary<string, AssetBundle> bundlesDict = new Dictionary<string, AssetBundle>();

        /// <summary>
        /// приоритетная очередь
        /// </summary>
        [SerializeField] private List<BundleLoaderBase> loadersQueue = new List<BundleLoaderBase>();
        public int LoadersQueueCount { get { return loadersQueue.Count; } }

        public bool needCleanUpAsserts = false;


        private Coroutine _loadingCoroutine = null;

        private void Awake()
        {
            ContainerHolder.Container.BuildUp(this);
            Debug.Log("url " + _config.AssetBundleUrl);

            if (_config.LoadBundlesFromStreamingAssets)
            {
                ROOT_URL = Application.streamingAssetsPath + '/';
            }
            else
            {
                if (_config.LoadBundlesFromFileSystem && Application.isEditor)
                {
                    ROOT_URL = "file://" + Application.dataPath + "/" + _config.EditorAssetBundleUrl; //"http://localhost:3000/unityBundles/";
                }
                else
                {
                    ROOT_URL = _config.AssetBundleUrl;
                }
            }
            if (Instance == null)
                Instance = this;

            Debug.Log("Bundles loaded from: " + ROOT_URL);

            LoadAllBundles();
        }

        public void CleanUpBundles()
        {
            foreach (var a in bundlesDict.Values)
            {
                a.Unload(true);
            }

            bundlesDict.Clear();
            OnBundlesCleanUp.Invoke();
        }

        private void LoadAllBundles()
        {
            StartCoroutine(LoadAllBundlesCoroutine());
        }


        private IEnumerator LoadAllBundlesCoroutine()
        {
            foreach (var bundleName in bundlesNamesToLoad)
            {               
                //если нужный бандл уже загружен, пропускаем
                if(IsBundleAlreadyLoaded(bundleName))
                    continue;

                yield return StartCoroutine(LoadConcreteBundleCoroutine(bundleName));
            }
        }

        /// <summary>
        /// Если есть элементы в очереди, нужно обслужить сперва их.
        /// <para>Элементы добавляются в очередь при регистрации RegisterBundleLoader()</para>
        /// </summary>
        /// <returns></returns>
        private IEnumerator ServeCurrentQueueCoroutine()
        {
            while (loadersQueue.Any())
            {
                var firstLoader = loadersQueue.First();
                var bundleName = firstLoader.BundleName;
                loadersQueue.Remove(firstLoader);

                if (IsBundleAlreadyLoaded(bundleName))
                    continue;

                yield return StartCoroutine(LoadConcreteBundleCoroutine(bundleName));
            }

            _loadingCoroutine = null;
        }

        private IEnumerator LoadConcreteBundleCoroutine(string bundleName)
        {
            while (!Caching.ready)
                yield return null;

            var url = GetBundleUrlByBuilder(bundleName);
            var www = WWW.LoadFromCacheOrDownload(url, _config.BundlesVersion);

            yield return www;     

            var bundle = www.assetBundle;

            if (bundle == null)
            {
                Debug.LogError(string.Format("Unable to load bundle {0} at url: {1} !", bundleName, url));
                yield break;
            }

            bundlesDict.Add(bundleName, bundle);
            OnBundleLoadedEvent(bundleName, bundle);
        }

        /// <summary>
        /// Загружен ли бандл
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        private bool IsBundleAlreadyLoaded(string bundleName)
        {
            return bundlesDict.Keys.Contains(bundleName);
        }
        
       
        private string GetBundleUrlByBuilder(string bundleName)
        {
            var strBuilder = new StringBuilder();
            strBuilder.Append(ROOT_URL);
            strBuilder.Append(bundleName);
            strBuilder.Append(FILE_EXTENSION);
            return strBuilder.ToString();
        }

        /// <summary>
        /// Может вернуть NULL если нет ключа.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public AssetBundle GetBundleByType(string type)
        {
            AssetBundle bundle = null;

            if (bundlesDict.ContainsKey(type))
                bundle = bundlesDict[type];

            return bundle;
        }

        public void RegisterBundleLoader(BundleLoaderBase loader)
        {
            if(loader.IsInPriory)
            {
                ClearListFromUnusing();
                loadersQueue.Insert(0, loader); //Вставляем элемент первым по списку
            }
            
            OnLoaderAddedEvent(loader);
            loader.LoadResource();

            if (_loadingCoroutine == null)
            {
                _loadingCoroutine = StartCoroutine(ServeCurrentQueueCoroutine());
            }
        }

        private void ClearListFromUnusing()
        {
            loadersQueue.RemoveAll(loader => loader == null);
        }
    }
}