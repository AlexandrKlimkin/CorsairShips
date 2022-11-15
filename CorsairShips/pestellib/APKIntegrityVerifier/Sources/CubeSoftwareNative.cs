using UnityEngine;

public class CubeSoftwareNative : MonoBehaviour 
{
    private static CubeSoftwareNative sInstance;

    public static CubeSoftwareNative Instance
    {
        get
        {
            if(sInstance == null)
            {
                GameObject obj = new GameObject(typeof(CubeSoftwareNative).Name);
                sInstance = obj.AddComponent<CubeSoftwareNative>();
                DontDestroyOnLoad(obj);
            }
            return sInstance;
        }
    }

    public bool ValidateSignature(string signature)
    {
        using(AndroidJavaClass cls = new AndroidJavaClass("com.cubesoftware.cubesoftwarenative.CubeSoftwareNative"))
            return cls.CallStatic<bool>("ValidateSignature", signature);
    }

    public string GetInstallerID()
    {
        using(AndroidJavaClass cls = new AndroidJavaClass("com.cubesoftware.cubesoftwarenative.CubeSoftwareNative"))
            return cls.CallStatic<string>("GetInstallerID");
    }

    public bool IsDebuggable()
    {
        using(AndroidJavaClass cls = new AndroidJavaClass("com.cubesoftware.cubesoftwarenative.CubeSoftwareNative"))
            return cls.CallStatic<bool>("IsDebuggable");
    }

    public string GetDirectory(string name)
    {
#if UNITY_EDITOR
        return "";
#else
        using(AndroidJavaClass cls = new AndroidJavaClass("com.cubesoftware.cubesoftwarenative.CubeSoftwareNative"))
            return cls.CallStatic<string>("GetDirectory", name);
#endif
    }
}