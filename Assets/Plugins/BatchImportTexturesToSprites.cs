using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BatchImportTexturesToSprites
{
    public static List<string> TexturesToSprites(string folderPath)
    {
        // enumerate all textures in the folder
        List<string> spriteFiles = new List<string>();
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.spritePackingTag = "UI";

                TextureImporterSettings importerSettings = new TextureImporterSettings();
                textureImporter.ReadTextureSettings(importerSettings);

                importerSettings.spriteMeshType = SpriteMeshType.FullRect;
                importerSettings.spriteExtrude = 0;
                importerSettings.spriteAlignment = (int)SpriteAlignment.Center;
                importerSettings.spritePivot = new Vector2(0.5f, 0.5f);
                importerSettings.readable = false;
                importerSettings.textureType = TextureImporterType.Sprite;
                importerSettings.spriteMode = (int)SpriteImportMode.Single;
                importerSettings.wrapMode = TextureWrapMode.Clamp;
                importerSettings.alphaSource = TextureImporterAlphaSource.FromInput;
                importerSettings.alphaIsTransparency = true;
                importerSettings.sRGBTexture = true;
                importerSettings.filterMode = FilterMode.Point;
                importerSettings.mipmapEnabled = false;
                importerSettings.npotScale = TextureImporterNPOTScale.None;
                importerSettings.aniso = 1;

                textureImporter.SetTextureSettings(importerSettings);
                textureImporter.SaveAndReimport();
            }
            spriteFiles.Add(path);
        }
        AssetDatabase.Refresh();
        return spriteFiles;
    }
}