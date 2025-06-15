using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class SpriteRenamerEditor : EditorWindow
{
    Sprite spriteToRename;
    string newSpriteName = "";

    [MenuItem("Tools/03 Rename Sprite", priority = 3)]
    public static void ShowWindow()
    {
        GetWindow<SpriteRenamerEditor>("Sprite Renamer");
    }

    private void OnGUI()
    {
        GUIStyle style = new(GUI.skin.label)
        {
            wordWrap = true
        };

        GUILayout.Label("Rename a Sprite in a Spritesheet", EditorStyles.boldLabel);
        GUILayout.Label("为图集中的一个图像重新命名", EditorStyles.boldLabel);
        GUILayout.Space(35);

        GUILayout.Label("Sprite*");
        spriteToRename = (Sprite)EditorGUILayout.ObjectField("图像*", spriteToRename, typeof(Sprite), false);
        GUILayout.Label("New Sprite Name*");
        newSpriteName = EditorGUILayout.TextField("新名称*", newSpriteName);
        GUILayout.Space(5);

        GUI.enabled = spriteToRename != null && !string.IsNullOrWhiteSpace(newSpriteName);
        if (GUILayout.Button("Run (运行)"))
        {
            Texture2D texture = spriteToRename.texture;
            string path = AssetDatabase.GetAssetPath(texture);
            CleanMetaNameFileIdTable(path);
            Rename(spriteToRename.name, newSpriteName, path);
            // RenameSpriteInSheet(spriteToRename, newSpriteName);
            // RemoveSpriteFromSheet(spriteToRename);
        }

        GUI.enabled = true;

        GUILayout.Space(35);

        string messageEn = "Usage: drag a Sprite from Spritesheet to the Sprite field and enter the new name." +
                            "The entire Spritesheet will be reimported and the selected Sprite will be renamed.";
        string messageZh = "用途：从图集中拖动一个要重新命名的图像到‘图像’区域。然后输入新的名称。" +
                            "整个图集会被先重新载入而且所选图像的名称会被修改。";

        GUILayout.Label(messageEn, style);
        GUILayout.Label(messageZh, style);
    }

    private static void Rename(string currentSpriteName, string newName, string assetPath)
    {
        string metaPath = assetPath + ".meta";

        if (!File.Exists(metaPath))
        {
            Debug.LogError("Meta file does not exist: " + metaPath);
            return;
        }

        // Read and process meta file lines
        string[] lines = File.ReadAllLines(metaPath);
        List<string> newLines = new();
        bool insideSprites = false;
        bool insideTable = false;

        foreach (var line in lines)
        {
            if (line.Trim() == "sprites:")
            {
                insideSprites = true;
                newLines.Add(line);
                continue;
            }

            if (line.Trim() == "nameFileIdTable:")
            {
                if (insideSprites)
                {
                    Debug.LogError($"Couldn't find {currentSpriteName}.");
                }
                insideTable = true;
                newLines.Add(line);
                continue;
            }

            if (insideSprites)
            {
                if (line.Trim().StartsWith("name"))
                {
                    string name = line.Trim().Replace("name: ", "");
                    if (name == currentSpriteName)
                    {
                        newLines.Add("      name: " + newName);
                        insideSprites = false;
                    }
                    else
                    {
                        newLines.Add(line);
                    }
                }
                else
                {
                    newLines.Add(line);
                }
            }
            else if (insideTable)
            {
                if (!line.StartsWith("  "))
                {
                    insideTable = false;
                    newLines.Add(line);
                }
                else if (line.Trim().StartsWith(currentSpriteName))
                {
                    string id = line.Replace(currentSpriteName + ": ", "");
                    newLines.Add("      " + newName + ": " + id);
                }
                else
                {
                    newLines.Add(line);
                }
            }
            else
            {
                newLines.Add(line);
            }
        }

        File.WriteAllLines(metaPath, newLines);
        Debug.Log($"Renamed Sprite {currentSpriteName} to \"{newName}\".");
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
    }

    public static void CleanMetaNameFileIdTable(string assetPath)
    {
        string metaPath = assetPath + ".meta";

        if (!File.Exists(metaPath))
        {
            Debug.LogError("Meta file does not exist: " + metaPath);
            return;
        }

        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null || importer.spriteImportMode != SpriteImportMode.Multiple)
        {
            Debug.LogError("TextureImporter not valid or not in Multiple mode.");
            return;
        }

        // Get current sprite names
        HashSet<string> validNames = new();
        foreach (var meta in importer.spritesheet)
        {
            validNames.Add(meta.name);
        }

        // Read and process meta file lines
        string[] lines = File.ReadAllLines(metaPath);
        List<string> newLines = new();
        bool insideTable = false;
        int removed = 0;

        foreach (var line in lines)
        {
            if (line.Trim() == "nameFileIdTable:")
            {
                insideTable = true;
                newLines.Add(line);
                continue;
            }

            if (insideTable)
            {
                Match match = Regex.Match(line, @"^\s+(.+?):\s*(-?\d+)");
                if (match.Success)
                {
                    string key = match.Groups[1].Value;
                    if (validNames.Contains(key))
                        newLines.Add(line);
                    else
                        removed++;
                }
                else if (line.StartsWith("  ")) // Still possibly inside block
                {
                    newLines.Add(line);
                }
                else
                {
                    insideTable = false;
                    newLines.Add(line);
                }
            }
            else
            {
                newLines.Add(line);
            }
        }

        File.WriteAllLines(metaPath, newLines);
        Debug.Log($"Cleaned {removed} dangling entries from nameFileIdTable in: {metaPath}");

        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
    }
}
