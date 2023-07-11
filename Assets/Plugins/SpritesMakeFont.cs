using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;
using UnityEditor.U2D;
using TMPro;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.TextCore;

namespace TMPro
{
    public class SpritesMakeFont
    {
        const string MENU_TITLE = "Assets/Generate Font From Sprites In Folder";

        [MenuItem(MENU_TITLE, true)]
        private static bool CanGen()
        {
            if (Selection.activeObject == null) return false;
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path)) return false;
            if (AssetDatabase.IsValidFolder(path) == false) return false;
            return true;
        }

        [MenuItem(MENU_TITLE, false, 64)]
        public static void GenAtlasForSpritesInFolder()
        {
            var folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            var configPath = Path.Combine(folderPath, "config.json");

            Dictionary<string, FontConfig> configDict = null;
            if (File.Exists(configPath))
            {
                var config = File.ReadAllText(configPath);
                configDict = LitJson.JsonMapper.ToObject<Dictionary<string, FontConfig>>(config);
            }

            var spritesPath = SimpleTexturePacker.SpritesPacker.Pack(folderPath, 2, 2048);

            var spriteObjs = AssetDatabase.LoadAllAssetsAtPath(spritesPath);

            float fontSize = 0;
            var sprites = new List<Sprite>();
            foreach (var spriteObj in spriteObjs)
            {
                if (spriteObj is Sprite)
                {
                    var sp = spriteObj as Sprite;
                    fontSize = Mathf.Max(fontSize, sp.rect.height);
                    sprites.Add(sp);
                }
            }

            var textureInfo = AssetDatabase.LoadAssetAtPath<Texture2D>(spritesPath);

            UnityEngine.Debug.Log("textureInfo: " + textureInfo.width + " " + textureInfo.height);

            var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");

            int fntSize = (int)fontSize;
            var atlasWidth = textureInfo.width;
            var atlasHeight = textureInfo.height;
            var atlasPadding = 2;
            var renderMode = GlyphRenderMode.RASTER;
            var atlasPopulationMode = AtlasPopulationMode.Dynamic;
            var enableMultiAtlasSupport = true;
            var tex_FileName = Path.GetFileName(folderPath) + " FNT";

            // 创建字体
            var fontAsset = TMP_FontAsset.CreateFontAsset(
                font,
                fntSize,
                atlasPadding,
                renderMode,
                atlasWidth,
                atlasHeight,
                atlasPopulationMode,
                enableMultiAtlasSupport);

            // 设置字符
            Dictionary<uint, Sprite> spriteDict = new Dictionary<uint, Sprite>();
            uint[] unicodes = new uint[sprites.Count];
            for (int i = 0; i < sprites.Count; i++)
            {
                var sp = sprites[i];
                var name = sp.name;
                var unicode = name[0];
                unicodes[i] = unicode;
                spriteDict.Add(unicode, sp);
            }

            if (!fontAsset.TryAddCharacters(unicodes))
            {
                UnityEngine.Debug.LogError("TryAddCharacters failed");
                return;
            }

            for (int i = 0; i < fontAsset.characterTable.Count; i++)
            {
                var unicode = fontAsset.characterTable[i].unicode;
                var glyphIndex = fontAsset.characterTable[i].glyphIndex;

                var sp = spriteDict[unicode];

                var glyph = fontAsset.glyphLookupTable[glyphIndex];
                glyph.scale = 1;

                glyph.glyphRect = new GlyphRect(
                    (int)sp.rect.x,
                    (int)sp.rect.y,
                    (int)sp.rect.width,
                    (int)sp.rect.height
                );

                var config = configDict != null && configDict.ContainsKey(sp.name) ? configDict[sp.name] : null;

                glyph.metrics = new GlyphMetrics(
                    config != null ? config.w : sp.rect.width,
                    config != null ? config.h : sp.rect.height,
                    config != null ? config.bx : 0,
                    config != null ? config.by : 0,
                    config != null ? config.ad : sp.rect.width
                );

                fontAsset.glyphLookupTable[glyphIndex] = glyph;
            }

            var newFaceInfo = fontAsset.faceInfo;
            // Debug.Log("newFaceInfo: " + JsonUtility.ToJson(newFaceInfo));

            newFaceInfo.baseline = fntSize;
            newFaceInfo.lineHeight = fntSize;
            newFaceInfo.ascentLine = fntSize;
            newFaceInfo.pointSize = fntSize;

            var fontType = typeof(TMP_FontAsset);
            var faceInfoProperty = fontType.GetProperty("faceInfo");
            faceInfoProperty.SetValue(fontAsset, newFaceInfo);

            // Add Font Atlas as Sub-Asset
            fontAsset.atlasTextures = new Texture2D[] { textureInfo };
            textureInfo.name = tex_FileName + " Atlas";

            // Create new Material and Add it as Sub-Asset
            Shader default_Shader = Shader.Find("TextMeshPro/Bitmap Custom Atlas"); // m_shaderSelection;
            Material tmp_material = new Material(default_Shader);
            tmp_material.name = tex_FileName + " Material";
            tmp_material.SetTexture(ShaderUtilities.ID_MainTex, textureInfo);
            fontAsset.material = tmp_material;

            AssetDatabase.CreateAsset(fontAsset, folderPath + " FNT.asset");
            AssetDatabase.AddObjectToAsset(tmp_material, fontAsset);
            AssetDatabase.SaveAssets();

            fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(folderPath + " FNT.asset");
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();

            Debug.Log("Generate atlas for sprites in folder: " + folderPath + " successfully!");
        }
    }
}