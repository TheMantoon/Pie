using UnityEngine;
using UnityEngine.UI;
using Pie.Core;
using System.IO;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;

namespace Pie.UI
{
    public class PlayerUIController : MonoBehaviour
    {
        public Text titleText, performersText, timeText, volumeText;
        public RawImage coverImage, playImage;
        public Texture2D placeholderCover, resumeSprite, pauseSprite;
        public Slider positionSlider;
        private bool isDragging;
        private float timer = 0.0f;
        private bool isLoaded = false;
        private string[] validExts = new[] { "mp3", "mp2", "mp1", "ogg", "flac", "aif", "aiff", "wav", "wma", "mod" };

        private void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
            List<string> music = new List<string>();
            music = GetAudioFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
#if UNITY_STANDALONE && !UNITY_EDITOR
            if (!string.IsNullOrEmpty(SingleInstanceManager.StartupFile)) OpenAudio(SingleInstanceManager.StartupFile);
#endif
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            DragDropHandler dragDropHandler = DragDropHandler.Instance;
            dragDropHandler.fileDropEvent += delegate (string[] paths)
            {
                for (int i = 0; i < paths.Length; ++i)
                {
                    if(IsValidExtension(Path.GetExtension(paths[i]))) OpenAudio(paths[i]);
                }
            };
#endif
        }
        
        private bool IsValidExtension(string ext)
        {
            if (string.IsNullOrEmpty(ext)) return false;
            ext = ext.TrimStart('.').ToLower();
            if (validExts.Length == 1 && validExts[0] == "*") return true;
            foreach (string valid in validExts) { if (ext == valid.ToLower()) return true; }
            return false;
        }

        private void Update()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            while (SingleInstanceManager.TryGetNextFile(out string path)) OpenAudio(path);
#endif
            timer += Time.deltaTime;
            if (timer >= 0.2f && !isDragging)
            {
                float progress = AudioPlayerService.Instance.GetPlaybackPosition();
                positionSlider.value = progress;
                UpdateTimeText(progress);
                timer = 0.0f;
                if (AudioPlayerService.Instance.GetState()) playImage.texture = pauseSprite;
                else playImage.texture = resumeSprite;
                string path = AudioPlayerService.Instance.GetPath();
                if ((!isLoaded && path != null) || (isLoaded && titleText.text != AudioMetadataService.GetTitle(path)))
                {
                    titleText.text = AudioMetadataService.GetTitle(path);
                    performersText.text = AudioMetadataService.GetPerformers(path);
                    coverImage.texture = AudioMetadataService.GetCover(path, placeholderCover);
                    positionSlider.interactable = true;
                    isLoaded = true;
                }
            }
        }

        public List<string> GetAudioFiles(string root)
        {
            List<string> result = new();
            if (!Directory.Exists(root)) return result;
            foreach (var ext in validExts)
            {
                try { result.AddRange(Directory.GetFiles(root, "*." + ext, SearchOption.AllDirectories)); }
                catch { }
            }
            return result;
        }

        public void OpenFile() => FilePickerService.Instance.PickAudioFile((path) => { OpenAudio(path); });

        private void OpenAudio(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            titleText.text = AudioMetadataService.GetTitle(path);
            performersText.text = AudioMetadataService.GetPerformers(path);
            coverImage.texture = AudioMetadataService.GetCover(path, placeholderCover);
            AudioPlayerService.Instance.Load(path);
            AudioPlayerService.Instance.Play();
            positionSlider.value = 0f;
            positionSlider.interactable = true;
            playImage.texture = pauseSprite;
            isLoaded = true;
        }

        public void Play()
        {
            AudioPlayerService.Instance.Play();
            positionSlider.value = 0f;
            positionSlider.interactable = true;
            if (isLoaded) playImage.texture = pauseSprite;
        }

        public void Pause()
        {
            if (!AudioPlayerService.Instance.GetState()) playImage.texture = pauseSprite;
            else playImage.texture = resumeSprite;
            AudioPlayerService.Instance.Pause();
        }

        public void Stop()
        {
            titleText.text = "Select audio";
            performersText.text = string.Empty;
            coverImage.texture = placeholderCover;
            AudioPlayerService.Instance.Stop();
            positionSlider.value = 0f;
            positionSlider.interactable = false;
            timeText.text = "00:00/00:00";
            playImage.texture = resumeSprite;
            isLoaded = false;
        }

        public void BeginDrag() => isDragging = true;

        public void EndDrag()
        {
            isDragging = false;
            AudioPlayerService.Instance.Seek(positionSlider.value);
        }

        public void OnVolumeChanged(float value)
        {
            AudioPlayerService.Instance.SetVolume(value);
            UpdateVolumeText(value);
        }

        public void OnLoopChanged(bool value) => AudioPlayerService.Instance.SetLoop(value);

        private void UpdateVolumeText(float value)
        {
            int percent = Mathf.RoundToInt(value * 100f);
            volumeText.text = $"Volume: {percent}%";
        }

        private void UpdateTimeText(float progress)
        {
            float current = progress * AudioPlayerService.Instance.GetTrackLength();
            float total = AudioPlayerService.Instance.GetTrackLength();
            timeText.text = $"{FormatTime(current)}/{FormatTime(total)}";
        }

        private string FormatTime(float seconds)
        {
            int min = Mathf.FloorToInt(seconds / 60f);
            int sec = Mathf.FloorToInt(seconds % 60f);
            return $"{min:00}:{sec:00}";
        }

#if UNITY_STANDALONE || UNITY_EDITOR
        private void OnApplicationQuit() => SingleInstanceManager.Shutdown();
#endif
    }
}