package com.pestelcrew.unitynotificationplugin;

import android.app.Activity;
import android.app.AlarmManager;
import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.res.Resources;
import android.graphics.BitmapFactory;
import android.graphics.Color;
import android.media.RingtoneManager;
import android.os.Build;
import android.service.notification.StatusBarNotification;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

public class UnityNotificationPlugin extends BroadcastReceiver {

    private static boolean _isNotificationsEnabled = true;

    public static void SetNotificationInTime(int id, long timestamp, String title, String message, String ticker, int sound, int vibrate, int lights, int bgColor, int executeMode, String unityClass){
        Activity currentActivity = UnityPlayer.currentActivity;
        AlarmManager alarmManager = (AlarmManager)currentActivity.getSystemService(Context.ALARM_SERVICE);
        Intent intent = new Intent(currentActivity, UnityNotificationPlugin.class);

        intent.putExtra("ticker", ticker);
        intent.putExtra("title", title);
        intent.putExtra("message", message);
        intent.putExtra("id", id);
        intent.putExtra("color", bgColor);
        intent.putExtra("sound", sound == 1);
        intent.putExtra("vibrate", vibrate == 1);
        intent.putExtra("lights", lights == 1);
        intent.putExtra("activity", unityClass);

        long delay = timestamp;

        PendingIntent pendingIntent = PendingIntent.getBroadcast(currentActivity, id, intent, 0);
        int mode = AlarmManager.RTC_WAKEUP;

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            if (executeMode == 2)
                alarmManager.setExactAndAllowWhileIdle(mode, delay, pendingIntent);
            else if (executeMode == 1)
                alarmManager.setExact(mode, delay, pendingIntent);
            else
                alarmManager.set(mode, delay, pendingIntent);
        } else {
            alarmManager.set(mode, delay, pendingIntent);
        }
    }

    public static void SetNotificationWithDelay(int id, long delayMs, String title, String message, String ticker, int sound, int vibrate, int lights, int bgColor, int executeMode, String unityClass){
        Activity currentActivity = UnityPlayer.currentActivity;
        AlarmManager alarmManager = (AlarmManager)currentActivity.getSystemService(Context.ALARM_SERVICE);
        Intent intent = new Intent(currentActivity, UnityNotificationPlugin.class);

        intent.putExtra("ticker", ticker);
        intent.putExtra("title", title);
        intent.putExtra("message", message);
        intent.putExtra("id", id);
        intent.putExtra("color", bgColor);
        intent.putExtra("sound", sound == 1);
        intent.putExtra("vibrate", vibrate == 1);
        intent.putExtra("lights", lights == 1);
        intent.putExtra("activity", unityClass);

        long delay = System.currentTimeMillis() + delayMs;
        PendingIntent pendingIntent = PendingIntent.getBroadcast(currentActivity, id, intent, 0);
        int mode = AlarmManager.RTC_WAKEUP;

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            if (executeMode == 2)
                alarmManager.setExactAndAllowWhileIdle(mode, delay, pendingIntent);
            else if (executeMode == 1)
                alarmManager.setExact(mode, delay, pendingIntent);
            else
                alarmManager.set(mode, delay, pendingIntent);
        } else {
            alarmManager.set(mode, delay, pendingIntent);
        }
    }

    public static void SetRepeatingNotificationWithDelay(int id, long delayMs, String title, String message, String ticker, long rep, int sound, int vibrate, int lights, int bgColor, String unityClass)
    {
        Activity currentActivity = UnityPlayer.currentActivity;
        AlarmManager alarmManager = (AlarmManager)currentActivity.getSystemService(Context.ALARM_SERVICE);
        Intent intent = new Intent(currentActivity, UnityNotificationPlugin.class);

        intent.putExtra("ticker", ticker);
        intent.putExtra("title", title);
        intent.putExtra("message", message);
        intent.putExtra("id", id);
        intent.putExtra("color", bgColor);
        intent.putExtra("sound", sound == 1);
        intent.putExtra("vibrate", vibrate == 1);
        intent.putExtra("lights", lights == 1);
        intent.putExtra("activity", unityClass);

        long delay = System.currentTimeMillis() + delayMs;
        PendingIntent pendingIntent = PendingIntent.getBroadcast(currentActivity, id, intent, 0);
        int mode = AlarmManager.RTC_WAKEUP;

        alarmManager.setRepeating(mode, delay, rep, pendingIntent);
    }

    public static void SetNotificationsEnabled(boolean isEnabled)
    {
        _isNotificationsEnabled = isEnabled;
    }

    public static void CancelNotification(int id) {
        Activity currentActivity = UnityPlayer.currentActivity;
        AlarmManager alarmManager = (AlarmManager)currentActivity.getSystemService(Context.ALARM_SERVICE);
        Intent intent = new Intent(currentActivity, UnityNotificationPlugin.class);
        PendingIntent pendingIntent = PendingIntent.getBroadcast(currentActivity, id, intent, 0);

        alarmManager.cancel(pendingIntent);
    }

    public static void CancelAll(){
        NotificationManager notificationManager = (NotificationManager)UnityPlayer.currentActivity.getApplicationContext().getSystemService(Context.NOTIFICATION_SERVICE);
        notificationManager.cancelAll();
    }

    @Override
    public void onReceive(Context context, Intent intent) {
        if (!_isNotificationsEnabled)
            return;

        NotificationManager notificationManager = (NotificationManager)context.getSystemService(Context.NOTIFICATION_SERVICE);
        Resources res = context.getResources();

        String ticker = intent.getStringExtra("ticker");
        String title = intent.getStringExtra("title");
        String message = intent.getStringExtra("message");
        String unityClass = intent.getStringExtra("activity");
        int id = intent.getIntExtra("id", 0);
        int color = intent.getIntExtra("color", 0);
        boolean sound = intent.getBooleanExtra("sound", false);
        boolean vibrate = intent.getBooleanExtra("vibrate", false);
        boolean lights = intent.getBooleanExtra("lights", false);

        Class<?> unityClassActivity = null;
        try {
            unityClassActivity = Class.forName(unityClass);
        } catch (ClassNotFoundException e)
        {
            e.printStackTrace();
            return;
        }

        Intent notificationIntent = new Intent(context, unityClassActivity);

        PendingIntent contentIntent = PendingIntent.getActivity(context, 0, notificationIntent, 0);
        Notification.Builder builder = new Notification.Builder(context);

        builder.setContentIntent(contentIntent)
                .setContentTitle(title)
                .setWhen(System.currentTimeMillis())
                .setAutoCancel(true)
                .setContentText(message);

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP)
            builder.setColor(color);

        if (ticker != null && ticker.length() > 0)
            builder.setTicker(ticker);

        int pushIconId = R.drawable.ic_push;

        builder.setSmallIcon(pushIconId);
        builder.setLargeIcon(BitmapFactory.decodeResource(res, R.drawable.ic_push_big));

        if (sound)
            builder.setSound(RingtoneManager.getDefaultUri(RingtoneManager.TYPE_NOTIFICATION));

        if (vibrate)
            builder.setVibrate(new long[] {1000L, 1000L});

        if (lights)
            builder.setLights(Color.GREEN, 3000, 3000);

        Notification notification = builder.build();

        notificationManager.notify(id, notification);
    }
}
