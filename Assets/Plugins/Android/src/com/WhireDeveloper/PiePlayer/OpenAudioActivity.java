package com.WhireDeveloper.PiePlayer;

import android.app.Activity;
import android.content.Intent;
import android.database.Cursor;
import android.net.Uri;
import android.os.Bundle;
import android.provider.OpenableColumns;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.InputStream;

public class OpenAudioActivity extends Activity {

    public static final String EXTRA_AUDIO_PATH = "com.WhireDeveloper.PiePlayer.AUDIO_PATH";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        Uri uri = getIntent().getData();
        if (uri != null) {
            String path = resolveUri(uri);
            if (path != null) {
                Intent launchIntent = new Intent(this, MainActivity.class);
                launchIntent.setAction(Intent.ACTION_VIEW);
                launchIntent.putExtra(EXTRA_AUDIO_PATH, path);
                launchIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_SINGLE_TOP | Intent.FLAG_ACTIVITY_CLEAR_TOP);
                startActivity(launchIntent);
            }
        }

        finish();
    }

    private String resolveUri(Uri uri) {
        if ("file".equalsIgnoreCase(uri.getScheme())) {
            return uri.getPath();
        }
        if (uri.getScheme() == null) {
            return uri.getPath();
        }
        if ("content".equalsIgnoreCase(uri.getScheme())) {
            return copyContentUriToCache(uri);
        }
        return null;
    }

    private String copyContentUriToCache(Uri uri) {
        String fileName = getFileName(uri);
        if (fileName == null || fileName.isEmpty()) {
            fileName = "opened_audio";
        }
        File cacheDir = new File(getCacheDir(), "opened_audio");
        if (!cacheDir.exists()) {
            cacheDir.mkdirs();
        }
        File outputFile = new File(cacheDir, fileName);
        try (
                InputStream input = getContentResolver().openInputStream(uri);
                FileOutputStream output = new FileOutputStream(outputFile)
        ) {
            if (input == null) {
                return null;
            }
            byte[] buffer = new byte[64 * 1024];
            int bytesRead;
            while ((bytesRead = input.read(buffer)) != -1) {
                output.write(buffer, 0, bytesRead);
            }
            output.flush();
            return outputFile.getAbsolutePath();

        } catch (Exception e) {
            e.printStackTrace();
            if (outputFile.exists()) {
                outputFile.delete();
            }
            return null;
        }
    }

    private String getFileName(Uri uri) {
        Cursor cursor = null;
        try {
            cursor = getContentResolver().query(uri, new String[]{OpenableColumns.DISPLAY_NAME}, null, null, null);
            if (cursor != null && cursor.moveToFirst()) {
                int index = cursor.getColumnIndex(OpenableColumns.DISPLAY_NAME);
                if (index >= 0) {
                    return cursor.getString(index);
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
        } finally {
            if (cursor != null) {
                cursor.close();
            }
        }
        return null;
    }
}