using System;

namespace SFB {
    public interface IStandaloneFileBrowser
    {
        public string[] OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect);
        public string[] OpenFolderPanel(string title, string directory, bool multiselect);
        public string SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions);
        public void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb);
        public void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<string[]> cb);
        public void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb);
    }
}