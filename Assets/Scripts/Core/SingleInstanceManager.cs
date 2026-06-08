#if UNITY_STANDALONE || UNITY_EDITOR
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;
using UnityEngine;

namespace Pie.Core
{
    public static class SingleInstanceManager
    {
        public static string StartupFile { get; private set; }
        public static bool IsMainInstance { get; private set; }
        private const int Port = 45782;
        private static TcpListener listener;
        private static Thread serverThread;
        private static bool running;
        private static readonly ConcurrentQueue<string> receivedFiles = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap() => Initialize();

        public static void Initialize()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1) StartupFile = args[1];
            try
            {
                listener = new TcpListener(IPAddress.Loopback, Port);
                listener.Start();
                IsMainInstance = true;
                StartServer();
            }
            catch (SocketException)
            {
                IsMainInstance = false;
                if (!string.IsNullOrEmpty(StartupFile))
                {
                    SendToMainInstance(StartupFile);
                    Application.Quit();
                }
            }
        }

        public static void Shutdown()
        {
            running = false;
            try { listener?.Stop(); listener = null; }
            catch { }
            try { serverThread?.Join(50); }
            catch { }
        }

        private static void StartServer()
        {
            running = true;
            serverThread = new Thread(ServerLoop) { IsBackground = true };
            serverThread.Start();
        }

        private static void ServerLoop()
        {
            while (running)
            {
                try
                {
                    var client = listener.AcceptTcpClient();
                    using (client)
                    using (var reader = new StreamReader(client.GetStream()))
                    {
                        string path = reader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(path)) receivedFiles.Enqueue(path);
                    }
                }
                catch { if (!running) break; }
            }
        }

        private static void SendToMainInstance(string path)
        {
            try
            {
                using var client = new TcpClient();
                client.Connect(IPAddress.Loopback, Port);
                using var writer = new StreamWriter(client.GetStream());
                writer.AutoFlush = true;
                writer.WriteLine(path);
            }
            catch { }
        }

        public static bool TryGetNextFile(out string path)
        {
            return receivedFiles.TryDequeue(out path);
        }
    }
}
#endif