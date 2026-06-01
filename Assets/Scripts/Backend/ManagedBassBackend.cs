#if UNITY_STANDALONE || UNITY_EDITOR
using ManagedBass;
using UnityEditor;

namespace Pie.Backend
{
    public class ManagedBassBackend : IAudioBackend
    {
        private int stream;
        private bool isLoop;
        private float volume = 1f;

        public ManagedBassBackend() => Bass.Init(-1, 48000, DeviceInitFlags.Default);

        public void Load(string path)
        {
            Stop();
            stream = Bass.CreateStream(path, 0);
            Bass.ChannelSetAttribute(stream, ChannelAttribute.Volume, volume);
            Bass.ChannelFlags(stream, isLoop ? BassFlags.Loop : BassFlags.Default, BassFlags.Loop);
            Play();
        }

        public void Play()
        {
            if (stream != 0) Bass.ChannelPlay(stream);
        }

        public void Pause(bool pause)
        {
            if (stream == 0) return;
            if (pause) Bass.ChannelPause(stream);
            else Bass.ChannelPlay(stream);
        }

        public void Stop()
        {
            if (stream != 0)
            {
                Bass.StreamFree(stream);
                stream = 0;
            }
        }

        public void SetVolume(float v)
        {
            volume = v;
            if (stream != 0) Bass.ChannelSetAttribute(stream, ChannelAttribute.Volume, v);
        }

        public void SetLoop(bool loop)
        {
            isLoop = loop;
            if (stream != 0) Bass.ChannelFlags(stream, loop ? BassFlags.Loop : BassFlags.Default, BassFlags.Loop);
        }

        public void Seek(float normalized)
        {
            if (stream == 0) return;
            normalized = UnityEngine.Mathf.Clamp01(normalized);
            long lengthBytes = Bass.ChannelGetLength(stream);
            long target = (long)(lengthBytes * normalized);
            Bass.ChannelSetPosition(stream, target);
        }

        public float GetPlaybackPosition()
        {
            if (stream == 0) return 0;
            long pos = Bass.ChannelGetPosition(stream);
            long len = Bass.ChannelGetLength(stream);
            if (len <= 0) return 0;
            return (float)pos / len;
        }

        public float GetTrackLength()
        {
            if (stream == 0) return 0;
            long lengthBytes = Bass.ChannelGetLength(stream);
            return (float)Bass.ChannelBytes2Seconds(stream, lengthBytes);
        }

        public bool GetState()
        {
            if (stream == 0) return false;
            return Bass.ChannelIsActive(stream) == PlaybackState.Playing;
        }

        ~ManagedBassBackend()
        {
            Stop();
            Bass.Free();
        }
    }
}

#if UNITY_EDITOR
[InitializeOnLoad]
public static class BassEditorCleanup
{
    static BassEditorCleanup() => EditorApplication.playModeStateChanged += OnPlayModeChanged;

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode) Bass.Free();
    }
}
#endif
#endif