/*
 * Decompiled with CFR 0.139.
 *
 * Could not load the following classes:
 *  android.app.Activity
 *  android.content.pm.ApplicationInfo
 *  android.content.pm.PackageInfo
 *  android.content.pm.PackageManager
 *  android.content.pm.Signature
 *  com.unity3d.player.UnityPlayer
 */
package com.cubesoftware.cubesoftwarenative;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.util.Log;

import com.unity3d.player.UnityPlayer;
import java.security.MessageDigest;

public class CubeSoftwareNative {
    public static boolean ValidateSignature(String signature) {
        try {
            Log.d("Unity", "IntegrityChecker: ValidateSignature");
            Activity context = UnityPlayer.currentActivity;
            PackageInfo packageInfo = context.getPackageManager().getPackageInfo(context.getPackageName(), PackageManager.GET_SIGNATURES);
            byte[] signatureByteArray = packageInfo.signatures[0].toByteArray();
            Log.d("Unity", "IntegrityChecker: signature length: " + signatureByteArray.length);
            String sha1buildSignature = CubeSoftwareNative.GetSHA1(signatureByteArray);
            Log.d("Unity", "IntegrityChecker: signature from config: " + signature + " signature from package manager: " + sha1buildSignature);
            return packageInfo.signatures.length == 1 && sha1buildSignature.equalsIgnoreCase(signature);
        }
        catch (Exception context) {
            Log.d("Unity", "IntegrityChecker: exception " + context);
            return false;
        }
    }

    public static String GetInstallerID() {
        try {
            Activity context = UnityPlayer.currentActivity;
            String installerID = context.getPackageManager().getInstallerPackageName(context.getPackageName());
            String result = installerID == null ? "" : installerID;
            Log.d("Unity", "IntegrityChecker: GetInstallerID: " + result);
            return result;
        }
        catch (Exception context) {
            Log.d("Unity", "IntegrityChecker: GetInstallerID exception: " + context);
            return "";
        }
    }

    public static boolean IsDebuggable() {
        try {
            Activity context = UnityPlayer.currentActivity;
            boolean isDebuggable = (context.getApplicationInfo().flags & 2) != 0;
            Log.d("Unity", "IntegrityChecker: isDebuggable: " + context);
            return isDebuggable;
        }
        catch (Exception context) {
            Log.d("Unity", "IntegrityChecker: isDebuggable exception: " + context);
            return false;
        }
    }

    public static String GetDirectory(String name) {
        try {
            Activity context = UnityPlayer.currentActivity;
            return context.getDir(name, 0).toString();
        }
        catch (Exception context) {
            return "";
        }
    }

    private static String GetSHA1(byte[] sig) {
        try {
            MessageDigest digest = MessageDigest.getInstance("SHA-1");
            if (digest == null) {
                Log.d("Unity", "IntegrityChecker: can't get SHA-1");
                return null;
            }
            digest.update(sig);
            return CubeSoftwareNative.BytesToHex(digest.digest());
        }
        catch (Exception context) {
            Log.d("Unity", "IntegrityChecker: GetSHA1 exception " + context);
            return null;
        }
    }

    private static String BytesToHex(byte[] bytes) {
        char[] hexArray = new char[]{'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
        char[] hexChars = new char[bytes.length * 2];
        for (int j = 0; j < bytes.length; ++j) {
            int v = bytes[j] & 255;
            hexChars[j * 2] = hexArray[v >>> 4];
            hexChars[j * 2 + 1] = hexArray[v & 15];
        }
        return new String(hexChars);
    }
}