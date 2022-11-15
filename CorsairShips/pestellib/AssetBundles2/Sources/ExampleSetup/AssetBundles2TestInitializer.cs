using System.Collections;
using System.Collections.Generic;
using Modules.BundleLoader;
using PestelLib.ClientConfig;
using UnityDI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AssetBundles2TestInitializer : MonoBehaviour {

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init()
	{
	    if (SceneManager.GetActiveScene().name != "ModelsPreview") return;

		ContainerHolder.Container.RegisterUnityScriptableObject<Config>();
	    ContainerHolder.Container.RegisterUnitySingleton<BundleManager>(null, true);
    }
}
