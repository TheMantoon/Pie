using UnityEngine;
using Pie.Backend;

namespace Pie.Core
{
    public class AudioPlayerService : MonoBehaviour
    {
        public static AudioPlayerService Instance;
        private IAudioBackend backend;
        public float Volume { get; private set; } = 1f;

        private void Awake()
        {
            Instance = this;
            Application.runInBackground = true;
#if UNITY_ANDROID && !UNITY_EDITOR
            backend = new AndroidExoBackend();
#elif UNITY_STANDALONE || UNITY_EDITOR
            backend = new ManagedBassBackend();
#endif
        }

        public void Load(string path) => backend.Load(path);
        public void Play() => backend.Play();
        public void Pause() => backend.Pause(GetState());
        public void Stop() => backend.Stop();

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