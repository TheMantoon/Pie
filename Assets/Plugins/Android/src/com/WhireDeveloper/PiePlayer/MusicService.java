package com.WhireDeveloper.PiePlayer;

import android.app.Service;
import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.content.Context;
import android.content.Intent;
import android.net.Uri;
import android.os.Build;
import android.os.IBinder;
import android.os.Handler;
import android.os.Looper;
import androidx.annotation.Nullable;
import androidx.media3.common.MediaItem;
import androidx.media3.exoplayer.ExoPlayer;

public class MusicService extends Service {

    private static final String CHANNEL_ID = "MusicServiceChannel";
    private static ExoPlayer player;
    private static boolean isLoop = false;
    private static volatile float cachedPosition = 0f;
    private static volatile float cachedDuration = 0f;
    private static volatile boolean cachedPlaying = false;

    private static final Handler mainHandler = new Handler(Looper.getMainLooper());

    private static void runOnMainThread(Runnable action) {
        mainHandler.post(action);
    }

    @Override
    public void onCreate() {
        super.onCreate();
        createNotificationChannel();
        if (player == null) {
            player = new ExoPlayer.Builder(this).build();
            mainHandler.post(stateUpdater);
        }
        startForeground(1, buildNotification());
    }

    public static void startService(Context context) {
        Intent intent = new Intent(context, MusicService.class);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            context.startForegroundService(intent);
        } else {
            context.startService(intent);
        }
    }

    private static final Runnable stateUpdater = new Runnable() {
        @Override
        public void run() {
            if (player != null) {
                long duration = player.getDuration();
                cachedDuration = duration > 0 ? duration / 1000f : 0f;
                cachedPosition = duration > 0 ? (float) player.getCurrentPosition() / duration : 0f;
                cachedPlaying = player.isPlaying();
            }
            mainHandler.postDelayed(this, 100);
        }
    };

    public static void play(String path) {
        runOnMainThread(() -> {
            if (player == null) {
                return;
            }
            MediaItem item = MediaItem.fromUri(Uri.parse(path));
            player.setMediaItem(item);
            player.prepare();
            player.play();
        });
    }

    public static void pause() {
        runOnMainThread(() -> {
            if (player != null) {
                player.pause();
            }
        });
    }

    public static void resume() {
        runOnMainThread(() -> {
            if (player != null) {
                player.play();
            }
        });
    }

    public static void stop() {
        runOnMainThread(() -> {
            if (player != null) {
                player.stop();
            }
        });
    }

    public static void setVolume(float volume) {
        runOnMainThread(() -> {
            if (player != null) {
                player.setVolume(volume);
            }
        });
    }

    public static void setLoop(boolean loop) {
        isLoop = loop;
        runOnMainThread(() -> {
            if (player != null) {
                player.setRepeatMode(loop ? ExoPlayer.REPEAT_MODE_ONE : ExoPlayer.REPEAT_MODE_OFF);
            }
        });
    }

    public static void seek(float normalized) {
        runOnMainThread(() -> {
            if (player == null) {
                return;
            }
            long duration = player.getDuration();
            if (duration <= 0) {
                return;
            }
            player.seekTo((long) (duration * normalized));
        });
    }

    public static float getPosition() {
        return cachedPosition;
    }

    public static float getDuration() {
        return cachedDuration;
    }

    public static boolean getState() {
        return cachedPlaying;
    }

    private Notification buildNotification() {
        return new Notification.Builder(this, CHANNEL_ID).setContentTitle("Pie Player").setContentText("Playing music")
                .setSmallIcon(android.R.drawable.ic_media_play).setOngoing(true).build();
    }

    private void createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationChannel channel = new NotificationChannel(CHANNEL_ID, "Music Service",
                    NotificationManager.IMPORTANCE_LOW);
            NotificationManager manager = getSystemService(NotificationManager.class);
            manager.createNotificationChannel(channel);
        }
    }

    @Nullable
    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }
}
