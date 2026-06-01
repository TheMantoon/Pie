using UnityEngine;
using UnityEngine.UI;
using Pie.Core;

namespace Pie.UI
{
    public class PlayerUIController : MonoBehaviour
    {
        public Text titleText, timeText, volumeText;
        public Image coverImage, playImage;
        public Sprite placeholderCover, resumeSprite, pauseSprite;
        public Slider positionSlider;
        private bool isDragging;
        private float timer = 0.0f;

        private void Start() => UpdateVolumeText(1f);

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= 0.2f && !isDragging)
            {
                float progress = AudioPlayerService.Instance.GetPlaybackPosition();
                positionSlider.value = progress;
                UpdateTimeText(progress);
                timer = 0.0f;
                if (AudioPlayerService.Instance.GetState()) playImage.sprite = pauseSprite;
                else playImage.sprite = resumeSprite;
            }
        }

        public void OpenFile()
        {
            FilePickerService.Instance.PickAudioFile((path) =>
            {
                if (string.IsNullOrEmpty(path)) return;
                titleText.text = AudioMetadataService.GetTitle(path);
                coverImage.sprite = AudioMetadataService.GetCover(path, placeholderCover);
                AudioPlayerService.Instance.Load(path);
                AudioPlayerService.Instance.Play();
                positionSlider.value = 0f;
                positionSlider.interactable = true;
                playImage.sprite = pauseSprite;
            });
        }

        public void Play()
        {
            AudioPlayerService.Instance.Play();
            positionSlider.value = 0f;
            positionSlider.interactable = true;
            playImage.sprite = pauseSprite;
        }

        public void Pause()
        {
            if (!AudioPlayerService.Instance.GetState()) playImage.sprite = pauseSprite;
            else playImage.sprite = resumeSprite;
            AudioPlayerService.Instance.Pause();
        }

        public void Stop()
        {
            titleText.text = "Select audio";
            coverImage.sprite = placeholderCover;
            AudioPlayerService.Instance.Stop();
            positionSlider.value = 0f;
            positionSlider.interactable = false;
            timeText.text = "00:00/00:00";
            playImage.sprite = resumeSprite;
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
    }
}