using System.Text;
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
    public class SpritesMakeFontConfig
    {
        const string MENU_TITLE = "Assets/Generate Font Config To Folder";

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
        public static void GenerateFontConfig()
        {
            var folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            var spritesPath = BatchImportTexturesToSprites.TexturesToSprites(folderPath);
            var configPath = Path.Combine(folderPath, "config.json");

            Dictionary<string, Sprite> spriteDict = new Dictionary<string, Sprite>();
            Dictionary<string, FontConfig> fontConfigDict = new Dictionary<string, FontConfig>();

            fontConfigDict.Add("default_comment", new FontConfig()
            {
                name = "default_comment",
                path = "name=char w=width h=height bx=x offset by=y offset ad=char width advance",
                w = 0,
                h = 0,
                bx = 0,
                by = 0,
                ad = 0,
            });

            float maxW = 0;
            float maxH = 0;
            foreach (var spPath in spritesPath)
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(spPath);
                var fontConfig = new FontConfig();
                fontConfig.name = sp.name;
                fontConfig.path = spPath;
                fontConfig.w = (int)sp.rect.width;
                fontConfig.h = (int)sp.rect.height;
                fontConfig.bx = (int)sp.bounds.center.x;
                fontConfig.by = (int)sp.bounds.center.y;
                fontConfig.ad = (int)sp.rect.width;
                fontConfigDict.Add(sp.name, fontConfig);

                maxW = Mathf.Max(maxW, sp.rect.width);
                maxH = Mathf.Max(maxH, sp.rect.height);

                spriteDict.Add(spPath, sp);
            }

            var jw = new LitJson.JsonWriter() { PrettyPrint = true };
            LitJson.JsonMapper.ToJson(fontConfigDict, jw);
            var json = jw.ToString();
            File.WriteAllText(configPath, json);
            AssetDatabase.Refresh();

            Debug.Log("Generate config for sprites in folder: " + folderPath + " successfully!");
        }
    }
}