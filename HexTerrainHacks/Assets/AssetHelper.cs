using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace HexTerrainHacks.Assets
{
    internal static class AssetHelper
    {
        // Bind the byte[] overload without resolving Unity's newer Span overloads.
        private static readonly Func<Texture2D, byte[], bool, bool> LoadImage =
            (Func<Texture2D, byte[], bool, bool>)Delegate.CreateDelegate(typeof(Func<Texture2D, byte[], bool, bool>),
                typeof(ImageConversion).GetMethod(nameof(ImageConversion.LoadImage),
                    new[] { typeof(Texture2D), typeof(byte[]), typeof(bool) }));

        internal static Sprite LoadSprite(string resourceName)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    if (Plugin.EnableDebugLogging)
                    {
                        Plugin.Log?.LogWarning($"[Assets] Embedded resource not found: {resourceName}");
                    }
                    return null;
                }

                using (var buffer = new MemoryStream())
                {
                    stream.CopyTo(buffer);
                    var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

                    if (!LoadImage(texture, buffer.ToArray(), false))
                    {
                        UnityEngine.Object.Destroy(texture);
                        if (Plugin.EnableDebugLogging)
                        {
                            Plugin.Log?.LogWarning($"[Assets] Failed to decode image: {resourceName}");
                        }
                        return null;
                    }

                    texture.name = resourceName;
                    texture.wrapMode = TextureWrapMode.Clamp;
                    var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f));
                    sprite.name = resourceName;
                    return sprite;
                }
            }
        }
    }
}
