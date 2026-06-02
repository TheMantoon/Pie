using UnityEngine;
using System.IO;

namespace Pie.Core
{
    public static class AudioMetadataService
    {
        public static string GetPerformers(string path)
        {
            var file = TagLib.File.Create(path);
            if (!string.IsNullOrEmpty(file.Tag.FirstPerformer)) return file.Tag.FirstPerformer;
            return string.Empty;
        }

        public static string GetTitle(string path)
        {
            var file = TagLib.File.Create(path);
            if (!string.IsNullOrEmpty(file.Tag.Title)) return file.Tag.Title;
            return Path.GetFileNameWithoutExtension(path);
        }

        public static Sprite GetCover(string path, Sprite placeholder)
        {
            var file = TagLib.File.Create(path);
            if (file.Tag.Pictures != null && file.Tag.Pictures.Length > 0)
            {
                byte[] data = file.Tag.Pictures[0].Data.Data;
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(data);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            return placeholder;
        }
    }
}