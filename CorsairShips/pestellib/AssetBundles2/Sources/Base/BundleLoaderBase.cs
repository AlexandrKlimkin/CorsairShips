using System;
using System.Collections;
using System.Collections.Generic;
using PestelLib.ClientConfig;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityDI;

namespace Modules.BundleLoader
{
    public abstract class BundleLoaderBase : MonoBehaviour
    {
        public static Dictionary<string, Object> AssetsCache = new Dictionary<string, Object>();
        public static Dictionary<string, int> bundlesCount = new Dictionary<string, int>();

        /// <summary>
        /// Имя ассета, который нужно загрузить
        /// </summary>
        public string assetName;

        /// <summary>
        /// Будет ли он исполняться в неочереди?
        /// </summary>
        [SerializeField] protected bool isInPriory = true;
        public bool IsInPriory => isInPriory;

        public string AssetName
        {
            get { return assetName; }
        }

        /// <summary>
        /// Имя бандла, в котором будет искаться ассет из BundleManager'a
        /// </summary>
        public string bundleName;

        public string BundleName
        {
            get { return bundleName; }
        }

        [Dependency] private BundleManager Manager;
        [Dependency] private Config _config;

        /// <summary>
        /// Бандл с ассетами, инициализация происходит в LoadResourceCoroutine()
        /// </summary>
        public AssetBundle Bundle { get; private set; }

        /// <summary>
        /// Упакованный ресурс, для получения нужного ассета нужно привести к нужному типу.
        /// </summary>
        protected abstract Object Resource { get; set; }

        protected void Start()
        {
            ContainerHolder.Container.BuildUp(this);
            if (_config.DontLoadAssetBundles)
            {
                enabled = false;
                return;
            }
            GetReferences();
            RegisterThisResourceInManager();
        }
        
        protected virtual void RegisterThisResourceInManager()
        {
            Manager.RegisterBundleLoader(this);
        }

        /// <summary>
        /// <para>Получить все необходимые ссылки для работы загрузчика:</para>
        /// например, если нужна текстура, то получить ссылку на материал и.т.д
        /// </summary>
        protected abstract void GetReferences();


        /// <summary>
        /// Непосредственно загрузка ресурса, должна ждать результатов инициализации свойства Resource из LoadResourceCoroutine
        /// <para>после этого нужно получить соответствующий ресурс путем приведения типов</para>
        /// <para>например нужна текстура, то в конце меняем текстуру в материале:</para>
        /// <para> Material.SetTexture("_MainTex", (Texture2D) Resource); </para>
        /// </summary>
        public abstract void LoadResource();

        /// <summary>
        /// Для последовательной загрузки
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerator LoadingProcessCoroutine();

        /// <summary>
        /// Ждет выполнения WaitingForLoadingBundleCoroutine, который инициализирует бандл в свойство Bundle
        /// <para>После этого загружает ассет из бандла и заполняет свойство Resource упакованным (Object) объектом</para>
        /// </summary>
        /// <typeparam name="T">Тип загружаемого ассета</typeparam>
        /// <returns></returns>
        protected IEnumerator LoadResourceCoroutine<T>()
        {
            yield return StartCoroutine(WaitingForLoadingBundleCoroutine<T>());
            var resourceRequest = Bundle.LoadAssetAsync(assetName, typeof(T));

            yield return resourceRequest;

            Resource = resourceRequest.asset;

            AssetsCache[AssetName] = Resource;
        }

        /// <summary>
        /// Пытается получить ссылку на бандл из менеджера по типу (свойство BundleType)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected IEnumerator WaitingForLoadingBundleCoroutine<T>()
        {
            while (Manager.GetBundleByType(this.bundleName) == null)
            {
                yield return null;
            }
                
            Bundle = Manager.GetBundleByType(this.bundleName);
        }

    }
}
