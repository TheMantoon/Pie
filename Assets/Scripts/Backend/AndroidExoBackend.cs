#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;

namespace Pie.Backend
{
    public class AndroidExoBackend : IAudioBackend
    {
        private AndroidJavaClass serviceClass;

        public AndroidExoBackend()
        {
            serviceClass = new AndroidJavaClass("com.WhireDeveloper.PiePlayer.MusicService");
            EnsureService();
        }

        public void Load(string path)
        {
            EnsureService();
            Stop();
            if (serviceClass == null) return;
            serviceClass.CallStatic("play", path);
        }

        public void Play()
        {
            EnsureService();
            if (serviceClass == null) return;
            serviceClass.CallStatic("resume");
        }

        public void Pause(bool p)
        {
            EnsureService();
            if (serviceClass == null) return;
            if (p) serviceClass.CallStatic("pause");
            else serviceClass.CallStatic("resume");
        }

        public void Stop()
        {
            EnsureService();
            if (serviceClass == null) return;
            serviceClass.CallStatic("stop");
        }

        public void SetVolume(float v)
        {
            EnsureService();
            if (serviceClass == null) return;
            serviceClass.CallStatic("setVolume", v);
        }

        public void SetLoop(bool loop)
        {
            EnsureService();
            if (serviceClass == null) return;
            serviceClass.CallStatic("setLoop", loop);
        }

        public void Seek(float n)
        {
            EnsureService();
            if (serviceClass == null) return;
            serviceClass.CallStatic("seek", n);
        }

        public float GetPlaybackPosition()
        {
            EnsureService();
            if (serviceClass == null) return 0f;
            return serviceClass.CallStatic<float>("getPosition");
        }

        public float GetTrackLength()
        {
            EnsureService();
            if (serviceClass == null) return 0f;
            return serviceClass.CallStatic<float>("getDuration");
        }

        public bool GetState()
        {
            EnsureService();
            if (serviceClass == null) return false;
            return serviceClass.CallStatic<bool>("getState");
        }

        private void EnsureService()
        {
            var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unity.GetStatic<AndroidJavaObject>("currentActivity");
            serviceClass.CallStatic("startService", activity);
        }
    }
}
#endif