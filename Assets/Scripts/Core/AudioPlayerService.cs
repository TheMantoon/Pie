using Pie.Backend;
using UnityEngine;

namespace Pie.Core
{
    public class AudioPlayerService : MonoBehaviour
    {
        public static AudioPlayerService Instance;
        private IAudioBackend backend;
        public float Volume { get; private set; } = 1f;
#if UNITY_ANDROID && !UNITY_EDITOR
        private const string OpenAudioExtra = "com.WhireDeveloper.PiePlayer.AUDIO_PATH";
#endif

        private void Awake()
        {
            Instance = this;
            UnityEngine.Application.runInBackground = true;
            backend = new ManagedBassBackend();
#if UNITY_ANDROID && !UNITY_EDITOR
            CheckOpenedAudio();
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void CheckOpenedAudio()
        {
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var intent = activity.Call<AndroidJavaObject>("getIntent");
                if (intent == null) return;
                string path = intent.Call<string>("getStringExtra", OpenAudioExtra);
                if (!string.IsNullOrEmpty(path))
                {
                    Load(path);
                    intent.Call<AndroidJavaObject>("removeExtra", OpenAudioExtra);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AudioPlayer] Failed to process opened audio: {e}");
            }
        }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        public void OnExternalAudioOpened(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            Load(path);
        }
#endif

        public void Load(string path) => backend.Load(path);

        public void Play()
        {
            backend.Play();
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidMusicService.Start();
#endif
        }

        public void Pause() => backend.Pause(GetState());

        public void Stop()
        {
            backend.Stop();
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidMusicService.Stop();
#endif
        }

        public void SetVolume(float v)
        {
            Volume = v;
            backend.SetVolume(v);
        }

        public void SetLoop(bool loop) => backend.SetLoop(loop);

        public void Seek(float n) => backend.Seek(n);

        public float GetPlaybackPosition() => backend.GetPlaybackPosition();

        public float GetTrackLength() => backend.GetTrackLength();

        public bool GetState() => backend.GetState();

        public string GetPath() => backend.GetPath();
    }
}