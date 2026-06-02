package com.WhireDeveloper.PiePlayer;

import android.content.Context;
import android.content.Intent;
import android.net.Uri;
import android.os.Build;
import android.os.Handler;
import android.os.Looper;
import android.graphics.BitmapFactory;
import android.media.MediaMetadataRetriever;
import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import androidx.core.app.NotificationCompat;
import androidx.annotation.Nullable;
import androidx.media3.common.MediaItem;
import androidx.media3.exoplayer.ExoPlayer;
import androidx.media3.session.MediaSession;
import androidx.media3.session.MediaSessionService;
import androidx.media3.common.MediaMetadata;

public class MusicService extends MediaSessionService {

    private static MusicService instance;
    private static ExoPlayer player;
    private MediaSession mediaSession;
    private static volatile float cachedPosition = 0f;
    private static volatile float cachedDuration = 0f;
    private static volatile boolean cachedPlaying = false;
    private static final Handler mainHandler = new Handler(Looper.getMainLooper());
    private static final String CHANNEL_ID = "PiePlayerChannel";
    private static final int NOTIFICATION_ID = 1;

    private static void runOnMainThread(Runnable action) {
        mainHandler.post(action);
    }

    @Override
    public void onCreate() {
        super.onCreate();
        instance = this;
        createNotificationChannel();
        if (player == null) {
            player = new ExoPlayer.Builder(this).build();
            mainHandler.post(stateUpdater);
        }
        mediaSession = new MediaSession.Builder(this, player).build();
        startForeground(NOTIFICATION_ID, buildNotification());
    }

    @Override
    public void onDestroy() {
        mainHandler.removeCallbacks(stateUpdater);
        if (mediaSession != null) {
            mediaSession.release();
            mediaSession = null;
        }
        if (player != null) {
            player.release();
            player = null;
        }
        instance = null;
        super.onDestroy();
    }

    @Nullable
    @Override
    public MediaSession onGetSession(androidx.media3.session.MediaSession.ControllerInfo controllerInfo) {
        return mediaSession;
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
            ExoPlayer p = player;
            if (p!= null) {
                long duration = p.getDuration();
                cachedDuration = duration > 0 ? duration / 1000f : 0f;
                cachedPosition = duration > 0 ? (float) p.getCurrentPosition() / duration : 0f;
                cachedPlaying = p.isPlaying();
            }
            mainHandler.postDelayed(this, 100);
        }
    };

    public static void play(String path) {
        runOnMainThread(() -> {
            ExoPlayer p = player;
            if (p == null) {
                return;
            }
            MediaItem mediaItem = createMediaItem(path);
            p.setMediaItem(mediaItem);
            p.prepare();
            p.play();
        });
    }

    public static void pause() {
        runOnMainThread(() -> {
            ExoPlayer p = player;
            if (p != null) {
                p.pause();
            }
        });
    }

    public static void resume() {
        runOnMainThread(() -> {
            ExoPlayer p = player;
            if (p != null) {
                p.play();
            }
        });
    }

    public static void stop() {
        runOnMainThread(() -> {
            ExoPlayer p = player;
            if (p != null) {
                p.stop();
                p.clearMediaItems();
            }
        });
    }

    public static void setVolume(float volume) {
        runOnMainThread(() -> {
            ExoPlayer p = player;
            if (p != null) {
                p.setVolume(volume);
            }
        });
    }

    public static void setLoop(boolean loop) {
        runOnMainThread(() -> {
            ExoPlayer p = player;
            if (p != null) {
                p.setRepeatMode(loop ? ExoPlayer.REPEAT_MODE_ONE : ExoPlayer.REPEAT_MODE_OFF);
            }
        });
    }

    public static void seek(float normalized) {

        runOnMainThread(() -> {
            ExoPlayer p = player;
            if (p == null) {
                return;
            }
            long duration = p.getDuration();
            if (duration <= 0) {
                return;
            }
            p.seekTo((long) (duration * normalized));
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

    private static MediaItem createMediaItem(String path) {
        String title = null;
        String artist = null;
        byte[] artwork = null;
        try {
            MediaMetadataRetriever retriever = new MediaMetadataRetriever();
            retriever.setDataSource(path);
            title = retriever.extractMetadata(MediaMetadataRetriever.METADATA_KEY_TITLE);
            artist = retriever.extractMetadata(MediaMetadataRetriever.METADATA_KEY_ARTIST);
            artwork = retriever.getEmbeddedPicture();
            retriever.release();
        } catch (Exception e) {
            e.printStackTrace();
        }
        if (title == null || title.isEmpty()) {
            java.io.File file = new java.io.File(path);
            title = file.getName();
        }
        MediaMetadata.Builder metadata = new MediaMetadata.Builder().setTitle(title);
        if (artist != null) {
            metadata.setArtist(artist);
        }
        if (artwork != null) {
            metadata.setArtworkData(artwork, MediaMetadata.PICTURE_TYPE_FRONT_COVER);
        }
        return new MediaItem.Builder().setUri(Uri.parse(path)).setMediaMetadata(metadata.build()).build();
    }

    private void createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationChannel channel = new NotificationChannel(CHANNEL_ID, "Music Service", NotificationManager.IMPORTANCE_LOW);
            NotificationManager manager = getSystemService(NotificationManager.class);
            manager.createNotificationChannel(channel);
        }
    }

    private Notification buildNotification() {
        Intent intent = new Intent(this, MainActivity.class);
        PendingIntent pendingIntent = PendingIntent.getActivity(this, 0, intent, PendingIntent.FLAG_IMMUTABLE);
        return new NotificationCompat.Builder(this, CHANNEL_ID).setContentTitle("Pie Player").setContentText("Service active")
        .setSmallIcon(android.R.drawable.ic_media_play).setContentIntent(pendingIntent).setOngoing(true).build();
    }
}