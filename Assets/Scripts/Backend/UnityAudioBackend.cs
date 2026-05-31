#if UNITY_STANDALONE || UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Pie.Core;

namespace Pie.Backend
{
    public class UnityAudioBackend : IAudioBackend
    {
        private AudioSource source;
        private AudioClip clip;
        private bool isLoaded;
        private bool isLoop;
        private float volume = 1f;

        public UnityAudioBackend()
        {
            GameObject go = new GameObject("UnityAudioBackend")
            {
                isStatic = true,
                layer = 2
            };
            source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
        }

        public void Load(string path)
        {
            Stop();
            AudioPlayerService.Instance.StartCoroutine(LoadClipCoroutine(path));
        }

        private IEnumerator LoadClipCoroutine(string path)
        {
            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            string url = $"file:///{directory.Replace("\\", "/")}/{Uri.EscapeDataString(fileName)}";
            using var request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success) yield break;
            clip = DownloadHandlerAudioClip.GetContent(request);
            source.clip = clip;
            source.loop = isLoop;
            source.volume = volume;
            isLoaded = true;
            Play();
        }

        public void Play()
        {
            if (!isLoaded || clip == null) return;
            source.Play();
        }

        public void Pause(bool pause)
        {
            if (!isLoaded) return;
            if (pause) source.Pause();
            else source.UnPause();
        }

        public void Stop()
        {
            if (source != null) source.Stop();
            if (clip != null) UnityEngine.Object.Destroy(clip);
            clip = null;
            isLoaded = false;
        }

        public void SetVolume(float v)
        {
            volume = v;
            if (source != null) source.volume = v;
        }

        public void SetLoop(bool loop)
        {
            isLoop = loop;
            if (source != null) source.loop = loop;
        }

        public void Seek(float normalized)
        {
            if (!isLoaded || clip == null) return;
            if (!source.isPlaying && GetPlaybackPosition() == 1) Play();
            source.time = clip.length * Mathf.Clamp01(normalized);
        }

        public float GetPlaybackPosition()
        {
            if (!isLoaded || clip == null || clip.length <= 0f) return 0f;
            return source.time / clip.length;
        }

        public float GetTrackLength()
        {
            return clip != null ? clip.length : 0f;
        }

        public bool GetState()
        {
            return source != null && source.isPlaying;
        }
    }
}
#endif