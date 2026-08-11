#if UNITY_STANDALONE_WIN
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SFB
{
    public class StandaloneFileBrowserWindows : IStandaloneFileBrowser
    {
        const int Size = 32768;

        [DllImport("PieFilePicker", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern int OpenFileDialog(string title, string directory, string filters, bool multi, IntPtr result, int size);

        [DllImport("PieFilePicker", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern int OpenFolderDialog(string title, string directory, bool multi, IntPtr result, int size);

        [DllImport("PieFilePicker", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern int SaveFileDialog(string title, string directory, string name, string filters, string extension, IntPtr result, int size);

        public string[] OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect)
        {
            IntPtr buffer = Alloc();
            try { return Read(OpenFileDialog(title, directory, Filter(extensions), multiselect, buffer, Size), buffer); }
            finally { Marshal.FreeHGlobal(buffer); }
        }

        public void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb) =>
            cb(OpenFilePanel(title, directory, extensions, multiselect));

        public string[] OpenFolderPanel(string title, string directory, bool multiselect)
        {
            IntPtr buffer = Alloc();
            try { return Read(OpenFolderDialog(title, directory, multiselect, buffer, Size), buffer); }
            finally { Marshal.FreeHGlobal(buffer); }
        }

        public void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<string[]> cb) =>
            cb(OpenFolderPanel(title, directory, multiselect));

        public string SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions)
        {
            IntPtr buffer = Alloc();
            try
            {
                return SaveFileDialog(title, directory, defaultName, Filter(extensions), DefaultExtension(extensions), buffer, Size) > 0
                    ? Marshal.PtrToStringUni(buffer) ?? "" : "";
            }
            finally { Marshal.FreeHGlobal(buffer); }
        }

        public void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb) =>
            cb(SaveFilePanel(title, directory, defaultName, extensions));

        static IntPtr Alloc()
        {
            IntPtr p = Marshal.AllocHGlobal(Size * 2);
            for (int i = 0; i < Size; i++) Marshal.WriteInt16(p, i * 2, 0);
            return p;
        }

        static string[] Read(int count, IntPtr p)
        {
            if (count <= 0) return Array.Empty<string>();
            var result = new string[count];
            long ptr = p.ToInt64();
            for (int i = 0; i < count; i++)
            {
                var s = new StringBuilder();
                while (true)
                {
                    char c = (char)Marshal.ReadInt16(new IntPtr(ptr));
                    ptr += 2;
                    if (c == 0) break;
                    s.Append(c);
                }
                result[i] = s.ToString();
            }
            return result;
        }

        static string Filter(ExtensionFilter[] extensions)
        {
            if (extensions == null || extensions.Length == 0) return "";
            var result = new StringBuilder();
            foreach (var filter in extensions)
            {
                if (filter.Extensions == null || filter.Extensions.Length == 0) continue;
                if (result.Length > 0) result.Append('\n');
                result.Append(filter.Name).Append('|');
                for (int i = 0; i < filter.Extensions.Length; i++)
                {
                    if (i > 0) result.Append(';');
                    string ext = filter.Extensions[i];
                    result.Append(ext.StartsWith(".") ? "*" + ext : "*." + ext);
                }
            }
            return result.ToString();
        }

        static string DefaultExtension(ExtensionFilter[] extensions)
        {
            if (extensions == null || extensions.Length == 0 || extensions[0].Extensions == null || extensions[0].Extensions.Length == 0) return "";
            string ext = extensions[0].Extensions[0];
            return ext.StartsWith(".") ? ext.Substring(1) : ext;
        }
    }
}
#endif