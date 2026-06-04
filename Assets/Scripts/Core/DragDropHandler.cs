#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Text;
using AOT;

namespace Pie.Core
{
    public delegate void FileDragDropDelegate(string[] filePaths);

    public class DragDropHandler : MonoBehaviour
    {
        public FileDragDropDelegate fileDropEvent = delegate { };
        public static DragDropHandler Instance;
        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lParam);
        private IntPtr hMainWindow;
        private static IntPtr oldWndProcPtr;
        private IntPtr newWndProcPtr;
        private WndProcDelegate newWndProc;
        private static IntPtr bestHandle;
        private const int WM_DROPFILES = 0x0233;
#if UNITY_EDITOR_WIN
        private const string UNITY_WND_CLASS = "UnityContainerWndClass";
#elif UNITY_STANDALONE_WIN
        private const string UNITY_WND_CLASS = "UnityWndClass";
#endif
        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern int GetClassNameW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)][Out] StringBuilder lpString, int nMaxCount);
        [DllImport("shell32.dll")]
        private static extern void DragAcceptFiles(IntPtr hWnd, bool fAccept);
        [DllImport("shell32.dll")]
        private static extern uint DragQueryFileW(IntPtr hDrop, uint iFile, [MarshalAs(UnmanagedType.LPWStr)][Out] StringBuilder filename, uint cch);
        [DllImport("shell32.dll")]
        private static extern void DragFinish(IntPtr hDrop);
        [DllImport("user32.dll")]
        private  static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsDelegate lpfn, IntPtr lParam);

        private void Awake()
        {
            if (Instance != null) return;
            hMainWindow = GetThreadWindow();
            if (hMainWindow == IntPtr.Zero) hMainWindow = GetActiveWindow();
            if (hMainWindow == IntPtr.Zero) return;
            newWndProc = new WndProcDelegate(WndProc);
            newWndProcPtr = Marshal.GetFunctionPointerForDelegate(newWndProc);
            oldWndProcPtr = SetWindowLongPtr(hMainWindow, -4, newWndProcPtr);
            DragAcceptFiles(hMainWindow, true);
            Instance = this;
        }

        private IntPtr GetThreadWindow()
        {
            uint currentThreadId = GetCurrentThreadId();
            EnumThreadWindows(currentThreadId, GetWindowHandle, IntPtr.Zero);
            return bestHandle;
        }

        [MonoPInvokeCallback(typeof(EnumWindowsDelegate))]
        private static bool GetWindowHandle(IntPtr hWnd, IntPtr lParam)
        {
            StringBuilder className = new StringBuilder(UNITY_WND_CLASS.Length);
            GetClassNameW(hWnd, className, className.Capacity + 1);
            if (className.ToString() == UNITY_WND_CLASS) bestHandle = hWnd;
            return true;
        }

        [MonoPInvokeCallback(typeof(WndProcDelegate))]
        private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_DROPFILES) Instance?.HandleFileDrop(wParam);
            return CallWindowProc(oldWndProcPtr, hWnd, msg, wParam, lParam);
        }

        private void HandleFileDrop(IntPtr wParam)
        {
            uint count = DragQueryFileW(wParam, 0xFFFFFFFF, null, 0);
            string[] filePaths = new string[count];
            for (uint i = 0; i < count; ++i)
            {
                uint size = DragQueryFileW(wParam, i, null, 0);
                StringBuilder path = new StringBuilder((int)size);
                DragQueryFileW(wParam, i, path, (uint)path.Capacity + 1);
                filePaths[i] = path.ToString();
            }
            fileDropEvent(filePaths);
            DragFinish(wParam);
        }

        private void OnDisable()
        {
            if (Instance == this)
            {
                SetWindowLongPtr(hMainWindow, -4, oldWndProcPtr);
                hMainWindow = IntPtr.Zero;
                oldWndProcPtr = IntPtr.Zero;
                newWndProcPtr = IntPtr.Zero;
                newWndProc = null;
                Instance = null;
            }
        }
    }
}
#endif