package com.WhireDeveloper.PiePlayer;

import android.content.Intent;
import android.os.Bundle;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

public class MainActivity extends UnityPlayerActivity {

    public static final String EXTRA_AUDIO_PATH = "com.WhireDeveloper.PiePlayer.AUDIO_PATH";

    @Override
    protected void onCreate(Bundle b) {
        super.onCreate(b);
    }

    @Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        setIntent(intent);
        String path = intent.getStringExtra(EXTRA_AUDIO_PATH);
        if (path != null && !path.isEmpty()) {
            UnityPlayer.UnitySendMessage("AudioPlayerService", "OnExternalAudioOpened", path);
        }
    }
}