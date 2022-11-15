package com.unity3d.plugin;

import android.app.Activity;
import android.app.Fragment;
import android.app.FragmentTransaction;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.provider.Settings;

public class UnityAndroidPermissions
{
    interface IPermissionRequestResult
    {
        void OnPermissionGranted(String permissionName);
        void OnPermissionDenied(String permissionName);
    }

    public boolean IsPermissionGranted (Activity activity, String permissionName)
    {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M)
            return true;
        if (activity == null)
            return false;
        return activity.checkSelfPermission(permissionName) == PackageManager.PERMISSION_GRANTED;
    }

    public boolean ShouldShowRequestPermission(Activity activity, String permissionName)
    {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M)
            return false;
        if (activity == null)
            return true;

        return activity.shouldShowRequestPermissionRationale(permissionName);
    }

    public void GoToAppSettings(Activity activity)
    {
        Intent intent = new Intent(Settings.ACTION_APPLICATION_DETAILS_SETTINGS);
        Uri uri = Uri.fromParts("package", activity.getPackageName(), null);
        intent.setData(uri);
        activity.startActivityForResult(intent, 1);
    }

    public void RequestPermissionAsync(Activity activity, final String[] permissionNames, final IPermissionRequestResult resultCallbacks)
    {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M)
            return;
        if (activity == null || permissionNames == null || resultCallbacks == null)
            return;

        final Fragment request = new PermissionFragment(resultCallbacks);
        Bundle bundle = new Bundle();
        bundle.putStringArray(PermissionFragment.PERMISSION_NAMES, permissionNames);
        request.setArguments(bundle);
        FragmentTransaction fragmentTransaction = activity.getFragmentManager().beginTransaction();
        fragmentTransaction.add(0, request);
        fragmentTransaction.commit();
    }
}
