#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;

namespace Pie.Backend
{
    public class AndroidExoBackend : IAudioBackend
    {
        private AndroidJavaClass serviceClass;

        public AndroidExoBackend()
        {
            var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unity.GetStatic<AndroidJavaObject>("currentActivity");
            serviceClass = new AndroidJavaClass("com.WhireDeveloper.PiePlayer.MusicService");
            serviceClass.CallStatic("startService", activity);
        }

        public void Load(string path)
        {
            Stop();
            if (serviceClass == null) return;
            serviceClass.CallStatic("play", path);
        }

        public void Play()
        {
            if (serviceClass == null) return;
            serviceClass.CallStatic("resume");
            Seek(0);
        }

        public void Pause(bool p)
        {
            if (serviceClass == null) return;
            if (p) serviceClass.CallStatic("pause");
            else serviceClass.CallStatic("resume");
        }

        public void Stop()
        {
            if (serviceClass == null) return;
            serviceClass.CallStatic("stop");
        }

        public void SetVolume(float v)
        {
            if (serviceClass == null) return;
            serviceClass.CallStatic("setVolume", v);
        }

        public void SetLoop(bool loop)
        {
            if (serviceClass == null) return;
            serviceClass.CallStatic("setLoop", loop);
        }

        public void Seek(float n)
        {
            if (serviceClass == null) return;
            serviceClass.CallStatic("seek", n);
        }

        public float GetPlaybackPosition()
        {
            if (serviceClass == null) return 0f;
            return serviceClass.CallStatic<float>("getPosition");
        }

        public float GetTrackLength()
        {
            if (serviceClass == null) return 0f;
            return serviceClass.CallStatic<float>("getDuration");
        }

        public bool GetState()
        {
            if (serviceClass == null) return false;
            return serviceClass.CallStatic<bool>("getState");
        }
    }
}
#endif