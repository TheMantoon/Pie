using UnityEngine;

namespace Pie.Core
{
    public class FilePickerService : MonoBehaviour
    {
        public static FilePickerService Instance;
        public string[] permissions;

        private void Awake() => Instance = this;

        private void Start()
        {
#if UNITY_ANDROID
            UnityEngine.Android.Permission.RequestUserPermissions(permissions);
            NativeFilePicker.RequestPermissionAsync();
#endif
        }

        public void PickAudioFile(System.Action<string> onComplete)
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            var paths = SFB.StandaloneFileBrowser.OpenFilePanel(
                "Select Audio", "", new[] { new SFB.ExtensionFilter("Audio", "mp3", "mp2", "mp1", "ogg", "flac", "aif", "aiff", "wav", 
                "wma", "mod") }, false);
            onComplete?.Invoke(paths.Length > 0 ? paths[0] : null);
#elif !UNITY_STANDALONE && !UNITY_EDITOR
            NativeFilePicker.PickFile((path) =>
            {
                if (string.IsNullOrEmpty(path))
                {
                    onComplete?.Invoke(null);
                    return;
                }
                onComplete?.Invoke(path);
            }, new[] { "audio/*" });
#endif
        }
    }
}