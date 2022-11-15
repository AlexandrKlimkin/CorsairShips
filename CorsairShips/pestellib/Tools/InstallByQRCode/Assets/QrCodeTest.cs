using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZXing;

public class QrCodeTest : MonoBehaviour {
    private WebCamTexture camTexture;
    private Rect screenRect;
    [SerializeField] private RawImage _rawImage;
    [SerializeField] private Text _textDebug;
    [SerializeField] private Text _uri;

    IEnumerator Start()
    {
        screenRect = new Rect(0, 0, Screen.width, Screen.height);
        camTexture = new WebCamTexture(480, 270, 20);
        //camTexture.requestedHeight = Screen.height;
        //camTexture.requestedWidth = Screen.width;
        if (camTexture != null)
        {
            camTexture.Play();
        }

        StartCoroutine(TryRead());

        yield return new WaitForSeconds(3f);
        Debug.Log("Android version: " + GetSDKLevel());
    }

    public static int GetSDKLevel()
    {
        var versionInfo = new AndroidJavaClass("android.os.Build$VERSION");
        Debug.Log("Version info: " + versionInfo);
        return versionInfo.GetStatic<int>("SDK_INT");
    }

    private IEnumerator TryRead()
    {
        while (true)
        {
            IBarcodeReader barcodeReader = new BarcodeReader();
            // decode the current frame
            var result = barcodeReader.Decode(camTexture.GetPixels32(),
                camTexture.width, camTexture.height);
            if (result != null)
            {
                Debug.Log("DECODED TEXT FROM QR: " + result.Text);
                _uri.text = result.Text;
                StartCoroutine(downLoadFromServer(result.Text));
                yield break;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public void ResetSystem()
    {
        SceneManager.LoadScene(0);
    }

    void Update()
    {
        _rawImage.texture = camTexture;
    }

    IEnumerator downLoadFromServer(string url)
    {
        //string url = "ftp://192.168.88.38/ModernWarplanes/Builds/312/1.8.25_312_ARMv7.apk";


        string savePath = Path.Combine(Application.persistentDataPath, "data");
        savePath = Path.Combine(savePath, "AntiOvr.apk");

        //Dictionary<string, string> header = new Dictionary<string, string>();
        //string userAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
        //header.Add("User-Agent", userAgent);
        WWW www = new WWW(url);


        while (!www.isDone)
        {
            //Must yield below/wait for a frame
            _textDebug.text = "Stat: " + www.progress;
            yield return null;
        }

        byte[] yourBytes = www.bytes;

        _textDebug.text = "Done downloading. Size: " + yourBytes.Length;


        //Create Directory if it does not exist
        if (!Directory.Exists(Path.GetDirectoryName(savePath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            _textDebug.text = "Created Dir";
        }

        try
        {
            //Now Save it
            System.IO.File.WriteAllBytes(savePath, yourBytes);
            Debug.Log("Saved Data to: " + savePath.Replace("/", "\\"));
            _textDebug.text = "Saved Data";
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed To Save Data to: " + savePath.Replace("/", "\\"));
            Debug.LogWarning("Error: " + e.Message);
            _textDebug.text = "Error Saving Data";
        }

        _textDebug.text = "Writing file to " + savePath;
        yield return new WaitForSeconds(3f);

        //Install APK
        if (GetSDKLevel() >= 24)
        {
            installAppAPI24(savePath);
        }
        else
        {
            installAppLegacy(savePath);
        }
    }

    private bool installAppLegacy(string apkPath)
    {
        try
        {
            AndroidJavaClass intentObj = new AndroidJavaClass("android.content.Intent");
            string ACTION_VIEW = intentObj.GetStatic<string>("ACTION_VIEW");
            int FLAG_ACTIVITY_NEW_TASK = intentObj.GetStatic<int>("FLAG_ACTIVITY_NEW_TASK");
            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", ACTION_VIEW);

            AndroidJavaObject fileObj = new AndroidJavaObject("java.io.File", apkPath);
            AndroidJavaClass uriObj = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject uri = uriObj.CallStatic<AndroidJavaObject>("fromFile", fileObj);

            intent.Call<AndroidJavaObject>("setDataAndType", uri, "application/vnd.android.package-archive");
            intent.Call<AndroidJavaObject>("addFlags", FLAG_ACTIVITY_NEW_TASK);
            intent.Call<AndroidJavaObject>("setClassName", "com.android.packageinstaller", "com.android.packageinstaller.PackageInstallerActivity");

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            currentActivity.Call("startActivity", intent);

            _textDebug.text = "Success";
            return true;
        }
        catch (System.Exception e)
        {
            _textDebug.text = "Error: " + e.Message;
            return false;
        }
    }

    //For API 24 and above
    private bool installAppAPI24(string apkPath)
    {
        bool success = true;
        _textDebug.text = "Installing App";


        //Get Activity then Context
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject unityContext = currentActivity.Call<AndroidJavaObject>("getApplicationContext");

        //Get the package Name
        string packageName = unityContext.Call<string>("getPackageName");
        string authority = packageName + ".fileprovider";

        AndroidJavaClass intentObj = new AndroidJavaClass("android.content.Intent");
        string ACTION_VIEW = intentObj.GetStatic<string>("ACTION_VIEW");
        AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", ACTION_VIEW);


        int FLAG_ACTIVITY_NEW_TASK = intentObj.GetStatic<int>("FLAG_ACTIVITY_NEW_TASK");
        int FLAG_GRANT_READ_URI_PERMISSION = intentObj.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION");

        //File fileObj = new File(String pathname);
        AndroidJavaObject fileObj = new AndroidJavaObject("java.io.File", apkPath);
        fileObj.Call<bool>("setReadable", true, false);

        //FileProvider object that will be used to call it static function
        AndroidJavaClass fileProvider = new AndroidJavaClass("android.support.v4.content.FileProvider");
        //getUriForFile(Context context, String authority, File file)
        AndroidJavaObject uri = fileProvider.CallStatic<AndroidJavaObject>("getUriForFile", unityContext, authority, fileObj);

        intent.Call<AndroidJavaObject>("setDataAndType", uri, "application/vnd.android.package-archive");
        intent.Call<AndroidJavaObject>("addFlags", FLAG_ACTIVITY_NEW_TASK);
        intent.Call<AndroidJavaObject>("addFlags", FLAG_GRANT_READ_URI_PERMISSION);
        currentActivity.Call("startActivity", intent);

        _textDebug.text = "Success";

        return success;
    }

    /*
    void OnGUI()
    {
        // drawing the camera on screen
        GUI.DrawTexture(screenRect, camTexture, ScaleMode.ScaleToFit);
        // do the reading — you might want to attempt to read less often than you draw on the screen for performance sake
        try
        {
            IBarcodeReader barcodeReader = new BarcodeReader();
            // decode the current frame
            var result = barcodeReader.Decode(camTexture.GetPixels32(),
                camTexture.width, camTexture.height);
            if (result != null)
            {
                Debug.Log("DECODED TEXT FROM QR: " + result.Text);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }*/
}
