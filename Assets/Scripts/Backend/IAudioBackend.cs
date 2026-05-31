namespace Pie.Backend
{
    public interface IAudioBackend
    {
        public void Load(string path);
        public void Play();
        public void Pause(bool pause);
        public void Stop();
        public void SetVolume(float volume);
        public void SetLoop(bool loop);
        public void Seek(float normalized);
        public float GetPlaybackPosition();
        public float GetTrackLength();
        public bool GetState();
    }
}