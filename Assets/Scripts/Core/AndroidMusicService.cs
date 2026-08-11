#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;
using System;
#endif

namespace Pie.Core
{
    public static class AndroidMusicService
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        private const string ServiceClass = "com.WhireDeveloper.PiePlayer.MusicService";

        public static void Start()
        {
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var service = new AndroidJavaClass(ServiceClass);
                service.CallStatic("start", activity);
            }
            catch (Exception e)
            {
                Debug.LogError($"[MusicService] Failed to start: {e.Message}");
            }
        }

        public static void Stop()
        {
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var service = new AndroidJavaClass(ServiceClass);
                service.CallStatic("stop", activity);
            }
            catch (Exception e)
            {
                Debug.LogError($"[MusicService] Failed to stop: {e.Message}");
            }
        }

#else

        public static void Start() { }

        public static void Stop() { }
#endif
    }
}