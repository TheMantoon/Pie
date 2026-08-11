package com.WhireDeveloper.PiePlayer;

import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.os.Build;
import android.os.IBinder;

public class MusicService extends Service {
    private static final String CHANNEL_ID = "pie_player_playback";
    private static final int NOTIFICATION_ID = 1001;
    public static final String ACTION_START = "com.WhireDeveloper.PiePlayer.START_MUSIC_SERVICE";
    public static final String ACTION_STOP = "com.WhireDeveloper.PiePlayer.STOP_MUSIC_SERVICE";

    public static void start(Context context) {
        Intent intent = new Intent(context, MusicService.class);
        intent.setAction(ACTION_START);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) context.startForegroundService(intent);
        else context.startService(intent);
    }

    public static void stop(Context context) {
        Intent intent = new Intent(context, MusicService.class);
        intent.setAction(ACTION_STOP);
        context.startService(intent);
    }

    @Override
    public void onCreate() {
        super.onCreate();
        createNotificationChannel();
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        if (intent != null && ACTION_STOP.equals(intent.getAction())) {
            stopForeground(true);
            stopSelf();
            return START_NOT_STICKY;
        }
        startForeground(NOTIFICATION_ID, createNotification());
        return START_STICKY;
    }

    private Notification createNotification() {
        Intent launchIntent = new Intent(this, MainActivity.class);
        launchIntent.setFlags(Intent.FLAG_ACTIVITY_SINGLE_TOP | Intent.FLAG_ACTIVITY_CLEAR_TOP);
        PendingIntent pendingIntent = PendingIntent.getActivity(this, 0, launchIntent, PendingIntent.FLAG_UPDATE_CURRENT | PendingIntent.FLAG_IMMUTABLE);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            return new Notification.Builder(this, CHANNEL_ID).setContentTitle("Pie Player").setContentText("Audio playing")
                .setSmallIcon(getApplicationInfo().icon).setContentIntent(pendingIntent).setOngoing(true)
                .setCategory(Notification.CATEGORY_TRANSPORT).build();
        }

        return new Notification.Builder(this)
            .setContentTitle("Pie Player").setContentText("Audio playing").setSmallIcon(getApplicationInfo().icon)
            .setContentIntent(pendingIntent).setOngoing(true).setCategory(Notification.CATEGORY_TRANSPORT).build();
    }

    private void createNotificationChannel() {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.O) return;
        NotificationChannel channel = new NotificationChannel(CHANNEL_ID, "Playing", NotificationManager.IMPORTANCE_LOW);
        channel.setDescription("Pie Player foreground playing");
        channel.setShowBadge(false);
        NotificationManager manager = getSystemService(NotificationManager.class);
        if (manager != null) manager.createNotificationChannel(channel);
    }

    @Override
    public void onDestroy() {
        stopForeground(true);
        super.onDestroy();
    }

    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }
}