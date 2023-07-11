using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine.U2D;
using System;

namespace SimpleTexturePacker
{
    /// <summary>
    /// Taken from: https://forum.unity.com/threads/text-mesh-pro-does-not-work-with-spriteatlas-assets.697088/#post-7581571
    /// All credits go to the original author
    /// Original name: SimpleSpriteAtlasConverter
    /// Atlas allowRotation and tightPacking must be set to false.
    /// Select a Sprite atlas and go to "Assets/SimpleSpriteAtlasConverter - Repack atlas to sprite" to convert it to a sprite.
    /// This is most useful to add a texture to TMPro without requiring managing a tpsheet project for TexturePacker
    /// </summary>
    public class SpritesPacker
    {
        public static string Pack(string folderPath, int padding, int maxTextureSize)
        {
            List<string> spriteFiles = BatchImportTexturesToSprites.TexturesToSprites(folderPath);

            // Pack sprites
            Sprite[] sprites = new Sprite[spriteFiles.Count];
            for (int i = 0; i < spriteFiles.Count; i++)
            {
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(spriteFiles[i]);
            }

            var rects = new List<SpriteRect>();
            foreach (var spr in sprites)
            {
                rects.Add(new SpriteRect(spr, padding));
            }
            rects.Sort((a, b) => b.area.CompareTo(a.area));

            var packer = new RectanglePacker();

            foreach (var rect in rects)
            {
                if (!packer.Pack(rect.w, rect.h, out rect.x, out rect.y))
                    throw new Exception("Uh oh, we couldn't pack the rectangle :(");
            }

            //Calculate image size
            var maxSize = maxTextureSize;
            var pngSize = Math.Max(packer.Width, packer.Height);
            var powoftwo = 16;
            while (powoftwo < pngSize) powoftwo *= 2;
            pngSize = powoftwo;
            if (pngSize > maxSize) pngSize = maxSize;

            RenderTextureReadWrite renderTextureReadWrite = RenderTextureReadWrite.sRGB;
            Texture2D texture = new Texture2D(pngSize, pngSize, TextureFormat.RGBA32, false);

            // Make texture transparent
            Color fillColor = Color.clear;
            Color[] fillPixels = new Color[texture.width * texture.height];
            for (int i = 0; i < fillPixels.Length; i++) fillPixels[i] = fillColor;
            texture.SetPixels(fillPixels);

            var metas = new List<SpriteMetaData>();

            // Draw sprites
            foreach (var rect in rects)
            {
                var t = GetReadableTexture(rect.sprite.texture, renderTextureReadWrite);
                texture.SetPixels32(rect.x + padding, rect.y + padding, (int)rect.sprite.rect.width, (int)rect.sprite.rect.height, t.GetPixels32());
                metas.Add(new SpriteMetaData()
                {
                    alignment = 6, //BottomLeft
                    name = rect.sprite.name.Replace("(Clone)", ""),
                    rect = new Rect(rect.x + padding, rect.y + padding, rect.sw, rect.sh)
                });
            }

            // Save image
            var pngPath = folderPath + ".png";

            Debug.Log($"Create sprite from sprites: {folderPath} to: {pngPath}");

            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(pngPath, bytes);

            // Update sprite settings
            AssetDatabase.Refresh();

            TextureImporter ti = AssetImporter.GetAtPath(pngPath) as TextureImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Multiple;
            ti.spritesheet = metas.ToArray();

            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();

            return pngPath;
        }

        private static Texture2D GetReadableTexture(Texture2D source, RenderTextureReadWrite readWriteMode)
        {
            RenderTexture tmp = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.ARGB32,
                readWriteMode);

            Graphics.Blit(source, tmp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;
            Texture2D result = new Texture2D(source.width, source.height);
            result.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            result.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);
            return result;
        }
    }
}