using UnityEngine;
using UnityDI;
using PestelLib.ClientConfig;
/*
    Класс для проверки подписи билда на андроиде

    Для того что бы получить SHA1 подпись билда, нужно разархивировать /META-INF/CERT.RSA 
    из вашего APK подписанного релизной подписью

    Дальше с помощью keytool входящей в java sdk получить SHA1 сertificate fingerprint, например:

    C:\Program Files\Java\jdk-10.0.1\bin>keytool -printcert -file c:\Users\nikolay\Downloads\CERT.RSA

Owner: C=Russia, ST=Saint-Petersburg, L=Saint-Petersburg, O=Cube Software, OU=Default Unit, CN=Sergey Petrov
Issuer: C=Russia, ST=Saint-Petersburg, L=Saint-Petersburg, O=Cube Software, OU=Default Unit, CN=Sergey Petrov
Serial number: 2b09f167
Valid from: Fri Dec 30 21:46:30 MSK 2016 until: Sat Dec 18 21:46:30 MSK 2066
Certificate fingerprints:
         SHA1: 23:AA:19:D5:2F:F0:D4:CF:4D:05:DE:9F:D0:35:FA:52:ED:97:C9:B5
         SHA256: 62:BF:84:26:F7:5F:37:D8:33:68:A9:EF:BE:C9:CA:FD:28:3B:F3:96:23:56:CE:51:80:50:11:BE:11:07:90:FF
Signature algorithm name: SHA1withRSA
Subject Public Key Algorithm: 1024-bit RSA key
Version: 3

    Полученный SHA1 нужно указать в поле PestelLib.ClientConfig.BuildSignature
 */

public class APKIntegrityChecker : MonoBehaviour
{
    private static bool? _cachedRespone = null;
    public static int errCode = 0;

    public static bool CanPlayWithOthers()
    {
        var config = ContainerHolder.Container.Resolve<Config>();
        return Check() || !config.UseIntegrityCheck;
    }

    /*
     * Возвращает true только для правильно подписанных билдов релизной подписью
     */
    public static bool Check()
    {
        if (Application.isEditor) return true;

#if UNITY_ANDROID
        if (_cachedRespone.HasValue)
        {
            return _cachedRespone.Value;
        }

        var config = ContainerHolder.Container.Resolve<Config>();
        var buildSignature = config.BuildSignature.Replace(":", string.Empty);

        Debug.Log("IntegrityChecker: signature: " + buildSignature);

        bool signatureValidated = CubeSoftwareNative.Instance.ValidateSignature(buildSignature);
        
        string installerID = CubeSoftwareNative.Instance.GetInstallerID();
        bool installerIDMatch = installerID == "com.android.vending";

        bool isDebuggable = CubeSoftwareNative.Instance.IsDebuggable();

        errCode = 0;

        if (!signatureValidated)
        {
            
            //AnalyticsManager.Instance.SimpletEvent("tamper");
            errCode |= 1;
            Debug.Log("IntegrityChecker: " + errCode);
        }

        if (!installerIDMatch)
        {
            //AnalyticsManager.Instance.OneParameterEvent("wild", "id", installerID);
            errCode |= 2;
            Debug.Log("IntegrityChecker: " + errCode);
        }

        if (isDebuggable)
        {
            //AnalyticsManager.Instance.SimpletEvent("debug");
            errCode |= 4;
            Debug.Log("IntegrityChecker: " + errCode);
        }

        _cachedRespone = (errCode == 0);

        return _cachedRespone.Value;
#else
        return true;
#endif
    }
}
